using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.LP3.Domain;
using Lykke.Service.LP3.Domain.Assets;
using Lykke.Service.LP3.Domain.Exchanges;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Services;
using Lykke.Service.LP3.Domain.Settings;
using MoreLinq;

namespace Lykke.Service.LP3.DomainServices
{
    public class Lp3Service : ILp3Service, IStartable, IDisposable
    {
        private readonly IOrderBookTraderService _orderBookTraderService;
        private readonly ILimitOrderService _limitOrderService;
        private readonly ILykkeExchange _lykkeExchange;
        private readonly IAssetsService _assetsService;
        private readonly IBalanceService _balanceService;
        private readonly ILog _log;
        
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        private readonly Timer _retryTimer;
        private readonly List<string> _retryNeededForTraders = new List<string>();

        public Lp3Service(ILogFactory logFactory,
            IOrderBookTraderService orderBookTraderService,
            ILimitOrderService limitOrderService,
            ILykkeExchange lykkeExchange,
            IAssetsService assetsService,
            IBalanceService balanceService)
        {
            _orderBookTraderService = orderBookTraderService;
            _limitOrderService = limitOrderService;
            _lykkeExchange = lykkeExchange;
            _assetsService = assetsService;
            _balanceService = balanceService;
            _log = logFactory.CreateLog(this);
            
            _retryTimer = new Timer(Retry, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }
        

        public void Start()
        {
            SynchronizeAsync(async () => await StartAsync()).GetAwaiter().GetResult();
        }
        
        public async Task HandleTradesAsync(IReadOnlyCollection<Trade> trades)
        {
            await SynchronizeAsync(async () =>
            {
                try
                {
                    if (!trades.Any()) return;

                    _log.Info("Trades received", context: $"trades: [{string.Join(", ", trades.Select(x => x.ToJson()))}]");

                    var assetPairId = trades.First().AssetPairId;
                    var trader = await _orderBookTraderService.GetTraderByAssetPairIdAsync(assetPairId);
                    var assetPairInfo = _assetsService.GetAssetPairInfo(assetPairId);

                    if (trader == null)
                    {
                        _log.Error($"No trader for {assetPairId}");
                        return;
                    }
                    
                    var (addedOrders, removedOrders) = trader.HandleTrades(trades, assetPairInfo.MinVolume);

                    await _orderBookTraderService.PersistOrderBookTraderAsync(trader);
                    
                    foreach (var removedOrder in removedOrders) await _limitOrderService.DeleteAsync(removedOrder.AssetPairId, removedOrder.Id);
                    foreach (var addedOrder in addedOrders) await _limitOrderService.AddAsync(addedOrder);

                    var allCurrentOrders = trader.GetOrders();

                    await _balanceService.UpdateBalancesAsync();

                    await ApplyOrdersAsync(assetPairId, allCurrentOrders, trader.CountInMarket);                    
                }
                catch (Exception e)
                {
                    _log.Error(e);
                }
            });
        }

        public async Task<IReadOnlyCollection<LimitOrder>> GetAllOrdersAsync()
        {
            return (await _orderBookTraderService.GetOrderBookTradersAsync()).SelectMany(x => x.GetOrders()).ToList();
        }

        public async Task<IReadOnlyCollection<LimitOrder>> GetOrdersForAssetAsync(string assetPairId)
        {
            return (await _orderBookTraderService.GetTraderByAssetPairIdAsync(assetPairId))?.GetOrders() ?? Array.Empty<LimitOrder>();
        }

        public async Task UpdateOrderBookTraderSettingsAsync(OrderBookTraderSettings orderBookTraderSettings)
        {
            await SynchronizeAsync(async () =>
            {
                await _orderBookTraderService.UpdateOrderBookTraderSettingsAsync(orderBookTraderSettings);
                var trader = await _orderBookTraderService.GetTraderByAssetPairIdAsync(orderBookTraderSettings.AssetPairId);
                await _limitOrderService.ClearAsync(trader.AssetPairId);
                var orders = trader.CreateOrders();
                await ApplyOrdersAsync(trader.AssetPairId, orders, trader.CountInMarket);    
            });
        }

        public async Task AddOrderBookTraderAsync(OrderBookTraderSettings orderBookTraderSettings)
        {
            await SynchronizeAsync(async () =>
                {
                    await _orderBookTraderService.AddOrderBookTraderAsync(orderBookTraderSettings);
                    var trader = await _orderBookTraderService.GetTraderByAssetPairIdAsync(orderBookTraderSettings.AssetPairId);
                    var orders = trader.CreateOrders();
                    await ApplyOrdersAsync(trader.AssetPairId, orders, trader.CountInMarket);
                    await _limitOrderService.AddOrUpdateBatchAsync(orders);
                });
        }

        public async Task DeleteOrderBookAsync(string assetPairId)
        {
            await SynchronizeAsync(async () =>
                {
                    await _lykkeExchange.ApplyAsync(assetPairId, Array.Empty<LimitOrder>());
                    await _orderBookTraderService.DeleteOrderBookAsync(assetPairId);
                    await _limitOrderService.ClearAsync(assetPairId);
                }
            );
        }
        
        public async Task ForceReplaceOrderBookAsync(string assetPairId)
        {
            await SynchronizeAsync(async () =>
            {
                var trader = await _orderBookTraderService.GetTraderByAssetPairIdAsync(assetPairId);
                if (trader == null)
                {
                    _log.Warning("OrderBook for recreate not found", context: $"assetPair: {assetPairId}");
                    return;
                }

                await _limitOrderService.ClearAsync(assetPairId);
                var orders = trader.GetOrders();
                await ApplyOrdersAsync(trader.AssetPairId, orders, trader.CountInMarket);
                await _limitOrderService.AddOrUpdateBatchAsync(orders);
            });
        }        

        public async Task AddOrderAsync(LimitOrder limitOrder)
        {
            await SynchronizeAsync(async () =>
            {
                var trader = await _orderBookTraderService.GetTraderByAssetPairIdAsync(limitOrder.AssetPairId);

                if (trader == null)
                {
                    _log.Warning("Can't add order, no such trader", context: $"order: {limitOrder.ToJson()}");
                    return;
                }

                limitOrder = trader.AddOrderManually(limitOrder);
                
                //await ApplySingleOrderAsync(limitOrder, trader.GetOrders());
                await ApplyOrdersAsync(limitOrder.AssetPairId, trader.GetOrders(), trader.CountInMarket);

                await _limitOrderService.AddAsync(limitOrder);
            });
        }

        public async Task CancelOrderAsync(string assetPairId, Guid orderId)
        {
            await SynchronizeAsync(async () =>
            {
                var order = (await _orderBookTraderService.GetTraderByAssetPairIdAsync(assetPairId))?.CancelOrder(orderId);
                if (order != null)
                    await ApplyCancelSingleOrderAsync(order);
                else
                    _log.Warning("Order for cancel not found", context: $"assetPair: {assetPairId}, id: {orderId}");

                await _limitOrderService.DeleteAsync(assetPairId, orderId);
            });
        }

        public async Task CancelAllOrdersAsync(string assetPairId)
        {
            await SynchronizeAsync(async () =>
            {
                (await _orderBookTraderService.GetTraderByAssetPairIdAsync(assetPairId))?.Clear();
                await _lykkeExchange.ApplyAsync(assetPairId, Array.Empty<LimitOrder>());
                await _limitOrderService.ClearAsync(assetPairId);
            });
        }

        public async Task<LimitOrder> RecreateOrderAsync(string assetPairId, Guid orderId)
        {
            return await SynchronizeAsyncWithResult(async () =>
            {
                var trader = await _orderBookTraderService.GetTraderByAssetPairIdAsync(assetPairId);

                if (trader == null)
                {
                    _log.Warning("Can't recreate order, no such trader", context: $"assetPair: {assetPairId}, id: {orderId}");
                    return null;
                }
                
                var order = trader.GetOrders().SingleOrDefault(x => x.Id == orderId);

                if (order != null)
                {
                    await ApplyCancelSingleOrderAsync(order);
                    await ApplySingleOrderAsync(order, trader.GetOrders());
                    await _limitOrderService.UpdateAsync(order);
                }
                else
                {
                    _log.Warning("Order for recreate not found", context: $"assetPair: {assetPairId}, id: {orderId}");
                }

                return order;
            });
        }

        public async Task SoftStopAsync(string assetPairId)
        {
            await SynchronizeAsync(async () =>
                {
                    var trader = await _orderBookTraderService.GetTraderByAssetPairIdAsync(assetPairId);
                    if (trader == null)
                    {
                        _log.Warning("OrderBook for soft stop not found", context: $"assetPair: {assetPairId}");
                        return;
                    }

                    trader.IsEnabled = false;
                    await _lykkeExchange.ApplyAsync(assetPairId, Array.Empty<LimitOrder>());
                    await _limitOrderService.AddOrUpdateBatchAsync(trader.GetOrders());
                    await _orderBookTraderService.PersistOrderBookTraderAsync(trader);
                });
        }

        public async Task SoftStartAsync(string assetPairId)
        {
            await SynchronizeAsync(async () =>
            {
                var trader = await _orderBookTraderService.GetTraderByAssetPairIdAsync(assetPairId);
                if (trader == null)
                {
                    _log.Warning("OrderBook for soft start not found", context: $"assetPair: {assetPairId}");
                    return;
                }

                trader.IsEnabled = true;
                await ApplyOrdersAsync(assetPairId, trader.GetOrders(), trader.CountInMarket);
                await _orderBookTraderService.PersistOrderBookTraderAsync(trader);
            });
        }

        public void Dispose()
        {
            _semaphore?.Dispose();
            _retryTimer?.Dispose();
        }

        private void Retry(object state)
        {
            SynchronizeAsync(async () =>
            {
                if (!_retryNeededForTraders.Any())
                    return;

                var assetPairId = _retryNeededForTraders.First();
                
                _log.Info($"Retrying placing order for {assetPairId}");

                var trader = await _orderBookTraderService.GetTraderByAssetPairIdAsync(assetPairId);
                if (trader != null) await ApplyOrdersAsync(trader.AssetPairId, trader.GetOrders(), trader.CountInMarket);
            }).GetAwaiter().GetResult();
        }

        private async Task StartAsync()
        {
            var traders = await _orderBookTraderService.GetOrderBookTradersAsync();
            var orders = await _limitOrderService.GetAllAsync();

            foreach (var trader in traders)
            {
                var ordersForTrader = orders.Where(x => x.AssetPairId == trader.AssetPairId).ToList();
                trader.RestoreOrders(ordersForTrader);
                _log.Info($"There were {ordersForTrader.Count} orders restored for trader {trader.AssetPairId}");
            }
        }

        private async Task SynchronizeAsync(Func<Task> asyncAction)
        {
            var lockTaken = false;
            try
            {
                lockTaken = await _semaphore.WaitAsync(Consts.LockTimeOut);
                if (!lockTaken)
                {
                    _log.Warning($"Can't take lock for {Consts.LockTimeOut}");
                    return;
                }

                await asyncAction();
            }
            catch(Exception e)
            {
                _log.Error(e);
                throw;
            }
            finally
            {
                if (lockTaken) _semaphore.Release();
            }
        }

        private async Task<TResult> SynchronizeAsyncWithResult<TResult>(Func<Task<TResult>> asyncAction)
        {
            var lockTaken = false;
            try
            {
                lockTaken = await _semaphore.WaitAsync(Consts.LockTimeOut);
                if (!lockTaken)
                {
                    _log.Warning($"Can't take lock for {Consts.LockTimeOut}");
                    return default;
                }

                return await asyncAction();
            }
            finally
            {
                if (lockTaken) _semaphore.Release();
            }
        }

        private async Task ApplySingleOrderAsync([NotNull] LimitOrder limitOrder, IReadOnlyCollection<LimitOrder> allOrders)
        {
            if (limitOrder == null) throw new ArgumentNullException(nameof(limitOrder));
            
            try
            {
                var assetPairInfo = _assetsService.GetAssetPairInfo(limitOrder.AssetPairId);
                limitOrder.Round(assetPairInfo);

                await ValidateBalanceForSingleOrderAsync(limitOrder, allOrders, assetPairInfo);

                if (limitOrder.Error == LimitOrderError.None)
                    await _lykkeExchange.PlaceLimitOrderAsync(limitOrder);
                else
                    _log.Info("Single order will not be placed as it have an error", context: $"order: {limitOrder.ToJson()}");
            }
            catch (Exception e)
            {
                _log.Error(e, "Error on placing single order", $"order: {limitOrder.ToJson()}");
                limitOrder.Error = LimitOrderError.Unknown;
                limitOrder.ErrorMessage = e.Message;
            }
        }

        private async Task ApplyCancelSingleOrderAsync([NotNull] LimitOrder limitOrder)
        {
            if (limitOrder == null) throw new ArgumentNullException(nameof(limitOrder));
            
            try
            {
                await _lykkeExchange.CancelLimitOrderAsync(limitOrder.ExternalId);
            }
            catch (Exception e)
            {
                _log.Error(e, "Error on cancelling single order", $"order: {limitOrder.ToJson()}");
            }
        }
        
        private async Task ApplyOrdersAsync(string assetPairId, IReadOnlyCollection<LimitOrder> orders, int countInMarket)
        {
            try
            {
                if (orders.Any(e => e.Error == LimitOrderError.OrderBookIsDisabled))
                    return;

                var assetPairInfo = _assetsService.GetAssetPairInfo(assetPairId);
                
                foreach (var limitOrder in orders)
                {
                    limitOrder.Round(assetPairInfo);

                    limitOrder.Error = LimitOrderError.None;
                    limitOrder.ErrorMessage = string.Empty;
                }

                await ValidateBalancesAsync(orders, assetPairInfo);
                
                _log.Info("ApplyingOrders from OrderBookTrader", 
                    context: $"assetPair: {assetPairId}," +
                             $"orders: [{string.Join(", ", orders.Select(x => x.ToJson()))}]");
                
                var success = false;
                
                try
                {
                    orders.Where(e => e.TradeType == TradeType.Buy).OrderByDescending(e => e.Price).Skip(countInMarket).ForEach(e =>
                    {
                        e.Error = LimitOrderError.NotInMarket;
                        e.ErrorMessage = "Order not in market";
                    });
                    orders.Where(e => e.TradeType == TradeType.Sell).OrderBy(e => e.Price).Skip(countInMarket).ForEach(e =>
                    {
                        e.Error = LimitOrderError.NotInMarket;
                        e.ErrorMessage = "Order not in market";
                    });
                    
                    var ordersToPlace = orders.Where(x => x.Error == LimitOrderError.None).ToList();
                    
                    await _lykkeExchange.ApplyAsync(assetPairId, ordersToPlace);
                    
                    if (ordersToPlace.All(x => x.Error == LimitOrderError.None))
                    {
                        _retryNeededForTraders.Remove(assetPairId);
                        success = true;    
                    }
                }
                catch (Exception e)
                {
                    _log.Error(e, $"Error on placing orders for {assetPairId}");
                }

                if (!success)
                    if (!_retryNeededForTraders.Contains(assetPairId)) _retryNeededForTraders.Add(assetPairId);

                if (_retryNeededForTraders.Any()) _retryTimer.Change(Consts.RetryPlacingOrdersPeriod, Timeout.InfiniteTimeSpan);
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
        }

        private async Task ValidateBalanceForSingleOrderAsync(LimitOrder order,
            IReadOnlyCollection<LimitOrder> currentOrders, AssetPairInfo assetPairInfo)
        {
            try
            {
                if (order.TradeType == TradeType.Sell)
                {
                    var availableBalance = (await _balanceService.GetByAssetIdAsync(assetPairInfo.BaseAssetId))?.Available ?? 0;
                    var currentlyUsedBalance = currentOrders
                        .Where(x => x != order)
                        .Where(x => x.TradeType == TradeType.Sell && x.Error == LimitOrderError.None)
                        .Sum(x => x.Volume);

                    if (currentlyUsedBalance + order.Volume > availableBalance)
                        order.Error = LimitOrderError.NotEnoughFunds;
                    else if (order.Error == LimitOrderError.NotEnoughFunds) order.Error = LimitOrderError.None;
                }
                else
                {
                    var availableBalance = (await _balanceService.GetByAssetIdAsync(assetPairInfo.QuoteAssetId))?.Available ?? 0;
                    var currentlyUsedBalance = currentOrders
                        .Where(x => x != order)
                        .Where(x => x.TradeType == TradeType.Buy && x.Error == LimitOrderError.None)
                        .Sum(x => x.Volume * x.Price);

                    if (currentlyUsedBalance + order.Volume * order.Price > availableBalance)
                        order.Error = LimitOrderError.NotEnoughFunds;
                    else if (order.Error == LimitOrderError.NotEnoughFunds) order.Error = LimitOrderError.None;
                }
            }
            catch (Exception e)
            {
                _log.Error(e, "Can't validate balance for single order", $"assetPairInfo: {assetPairInfo.ToJson()}");
            }
        }

        private async Task ValidateBalancesAsync(IReadOnlyCollection<LimitOrder> orders, AssetPairInfo assetPairInfo)
        {
            try
            {
                var baseBalance = await _balanceService.GetByAssetIdAsync(assetPairInfo.BaseAssetId);
                var balance = baseBalance.Available;

                foreach (var order in orders.Where(x => x.TradeType == TradeType.Sell).OrderBy(x => x.Price))
                {
                    balance -= order.Volume;
    
                    if (balance < 0)
                        order.Error = LimitOrderError.NotEnoughFunds;
                    else if (order.Error == LimitOrderError.NotEnoughFunds) order.Error = LimitOrderError.None;
                }
            }
            catch (Exception e)
            {
                _log.Error(e, "Can't validate balances for sell orders", $"assetPairInfo: {assetPairInfo.ToJson()}");
            }
            
            try
            {
                var quoteBalance = await _balanceService.GetByAssetIdAsync(assetPairInfo.QuoteAssetId);
                var balance = quoteBalance.Available;

                foreach (var order in orders.Where(x => x.TradeType == TradeType.Buy).OrderByDescending(x => x.Price))
                {
                    balance -= order.Volume * order.Price;
                
                    if (balance < 0)
                        order.Error = LimitOrderError.NotEnoughFunds;
                    else if (order.Error == LimitOrderError.NotEnoughFunds) order.Error = LimitOrderError.None;
                }
            }
            catch (Exception e)
            {
                _log.Error(e, "Can't validate balances for buy orders", $"assetPairInfo: {assetPairInfo.ToJson()}");
            }
        }
    }
}

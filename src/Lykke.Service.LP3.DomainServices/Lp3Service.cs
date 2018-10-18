using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.LP3.Domain;
using Lykke.Service.LP3.Domain.Assets;
using Lykke.Service.LP3.Domain.Exchanges;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Services;
using Lykke.Service.LP3.Domain.TradingAlgorithm;

namespace Lykke.Service.LP3.DomainServices
{
    public class Lp3Service : ILp3Service, IStartable
    {
        private readonly IOrderBookTraderService _orderBookTraderService;
        private readonly ILykkeExchange _lykkeExchange;
        private readonly IAssetsService _assetsService;
        private readonly IBalanceService _balanceService;
        private readonly ILog _log;
        
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        private readonly ConcurrentDictionary<string, IReadOnlyCollection<LimitOrder>> _ordersByAssetPairs = 
            new ConcurrentDictionary<string, IReadOnlyCollection<LimitOrder>>();
        
        public Lp3Service(ILogFactory logFactory,
            IOrderBookTraderService orderBookTraderService,
            ILykkeExchange lykkeExchange,
            IAssetsService assetsService,
            IBalanceService balanceService)
        {
            _orderBookTraderService = orderBookTraderService;
            _lykkeExchange = lykkeExchange;
            _assetsService = assetsService;
            _balanceService = balanceService;
            _log = logFactory.CreateLog(this);
        }
        

        public void Start()
        {
            SynchronizeAsync(async () => await StartAsync()).GetAwaiter().GetResult();
        }

        private async Task StartAsync()
        {
            var traders = await _orderBookTraderService.GetOrderBookTradersAsync();

            foreach (var trader in traders)
            {
                await ApplyOrdersAsync(trader);
            }
        }
        
        public async Task HandleTradesAsync(IReadOnlyCollection<Trade> trades)
        {
            await SynchronizeAsync(async () =>
            {
                try
                {
                    if (!trades.Any())
                    {
                        return;
                    }

                    var assetPairId = trades.First().AssetPairId;
                    var trader = (await _orderBookTraderService.GetOrderBookTradersAsync()).SingleOrDefault(x =>
                        string.Equals(x.AssetPairId, assetPairId, StringComparison.InvariantCultureIgnoreCase));

                    if (trader == null)
                    {
                        _log.Error($"No trader for {assetPairId}");
                        return;
                    }
                    
                    trader.HandleTrades(trades);

                    await _orderBookTraderService.UpdateOrderBookTraderAsync(trader);
                    
                    await ApplyOrdersAsync(trader);
                }
                catch (Exception e)
                {
                    _log.Error(e);
                }
            });
        }

        public IReadOnlyCollection<LimitOrder> GetOrders()
        {
            return _ordersByAssetPairs.SelectMany(x => x.Value).ToList();
        }

        private async Task SynchronizeAsync(Func<Task> asyncAction)
        {
            bool lockTaken = false;
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
            finally
            {
                if (lockTaken)
                {
                    _semaphore.Release();
                }
            }
        }

        private async Task ApplyOrdersAsync(OrderBookTrader trader)
        {
            try
            {
                var orders = trader.GetOrders();
                var assetPairInfo = _assetsService.GetAssetPairInfo(trader.AssetPairId);
                foreach (var limitOrder in orders)
                {
                    limitOrder.Round(assetPairInfo);
                }

                await ValidateBalancesAsync(orders, assetPairInfo);
                
                _ordersByAssetPairs[trader.AssetPairId] = orders;

                if (trader.IsEnabled)
                {
                    try
                    {
                        await _lykkeExchange.ApplyAsync(trader.AssetPairId, 
                            orders.Where(x => x.Error == LimitOrderError.None).ToList());
                    }
                    catch (Exception e)
                    {
                        _log.Error(e, $"Error on placing orders for {trader.AssetPairId}");
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
        }

        private async Task ValidateBalancesAsync(IReadOnlyCollection<LimitOrder> orders, AssetPairInfo assetPairInfo)
        {
            try
            {
                var baseBalance = await _balanceService.GetByAssetIdAsync(assetPairInfo.BaseAssetId);
                var balance = baseBalance.Amount;

                foreach (var order in orders.Where(x => x.TradeType == TradeType.Sell).OrderBy(x => x.Price))
                {
                    balance -= order.Volume;
    
                    if (balance < 0)
                    {
                        order.Error = LimitOrderError.NotEnoughFunds;
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error(e, "Can't validate balances for sell orders", context: $"assetPairInfo: {assetPairInfo.ToJson()}");
            }
            
            try
            {
                var quoteBalance = await _balanceService.GetByAssetIdAsync(assetPairInfo.QuoteAssetId);
                var balance = quoteBalance.Available;

                foreach (var order in orders.Where(x => x.TradeType == TradeType.Buy).OrderByDescending(x => x.Price))
                {
                    balance -= order.Volume * order.Price;
                
                    if (balance < 0)
                    {
                        order.Error = LimitOrderError.NotEnoughFunds;
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error(e, "Can't validate balances for buy orders", context: $"assetPairInfo: {assetPairInfo.ToJson()}");
            }
        }
    }
}

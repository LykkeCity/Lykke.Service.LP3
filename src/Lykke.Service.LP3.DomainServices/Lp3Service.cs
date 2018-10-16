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
using Lykke.Service.LP3.Domain.Exchanges;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Services;
using Lykke.Service.LP3.Domain.Settings;

namespace Lykke.Service.LP3.DomainServices
{
    public class Lp3Service : ILp3Service, IStartable
    {
        private readonly ISettingsService _settingsService;
        private readonly ILevelsService _levelsService;
        private readonly IAdditionalVolumeService _additionalVolumeService;
        private readonly ILykkeExchange _lykkeExchange;
        private readonly IOrdersConverter _ordersConverter;
        private readonly ITradesConverter _tradesConverter;
        private readonly ILog _log;
        
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private bool _started;

        private readonly ConcurrentDictionary<string, OrderBook> _ordersByAssetPairs = 
            new ConcurrentDictionary<string, OrderBook>();
        
        private string _baseAssetPairId;

        public Lp3Service(ILogFactory logFactory,
            ISettingsService settingsService,
            ILevelsService levelsService,
            IAdditionalVolumeService additionalVolumeService,
            ILykkeExchange lykkeExchange,
            IOrdersConverter ordersConverter,
            ITradesConverter tradesConverter)
        {
            _settingsService = settingsService;
            _levelsService = levelsService;
            _additionalVolumeService = additionalVolumeService;
            _lykkeExchange = lykkeExchange;
            _ordersConverter = ordersConverter;
            _tradesConverter = tradesConverter;
            _log = logFactory.CreateLog(this);
        }
        

        public void Start()
        {
            SynchronizeAsync(async () => await StartAsync()).GetAwaiter().GetResult();
        }

        private async Task StartAsync()
        {
            var initialPrice = await _settingsService.GetInitialPriceAsync();
            if (initialPrice == null)
            {
                _log.Info("No initial price to start algorithm, waiting for adding one via API");
                return;
            }
        
            _baseAssetPairId = (await _settingsService.GetBaseAssetPairSettingsAsync())?.AssetPairId;
            if (_baseAssetPairId == null)
            {
                _log.Info("No baseAssetPairId to start algorithm, waiting for adding it via API");
                return;
            }

            _levelsService.UpdateReference(initialPrice.Price);

            _started = true;
            
            await ApplyOrdersAsync();
        }
        
        public async Task HandleTradesAsync(IReadOnlyList<Trade> trades)
        {
            await SynchronizeAsync(async () =>
            {
                try
                {
                    var convertedTrades = await ConvertTrades(trades);

                    if (convertedTrades.Any(x => x.Type == TradeType.Sell))
                    {
                        var sellTrades = convertedTrades.Where(x => x.Type == TradeType.Sell).ToList();
                        await HandleTradeAsync(sellTrades);
                    }

                    if (convertedTrades.Any(x => x.Type == TradeType.Buy))
                    {
                        var buyTrades = convertedTrades.Where(x => x.Type == TradeType.Buy).ToList();
                        await HandleTradeAsync(buyTrades);
                    }

                    await ApplyOrdersAsync();
                }
                catch (Exception e)
                {
                    _log.Error(e);
                }
            });
        }

        private async Task UpdateInitialPrice(decimal price)
        {
            await _settingsService.AddOrUpdateInitialPriceAsync(price);

            _log.Info("InitialPrice is updated", context: $"new InitialPrice: {price}");
        }

        private async Task<IReadOnlyList<Trade>> ConvertTrades(IReadOnlyList<Trade> trades)
        {
            try
            {
                if (trades.First().AssetPairId == _baseAssetPairId)
                {
                    return trades;
                }
                
                var assetPairSettings = (await _settingsService.GetDependentAssetPairsSettingsAsync())
                    .SingleOrDefault(x => x.AssetPairId == trades.First().AssetPairId);
                
                return (await _tradesConverter.ConvertAsync(trades, assetPairSettings)).ToList();
            }
            catch (Exception e)
            {
                _log.Error(e, "Error on converting trades");
                throw;
            }
        }

        public async Task HandleTimerAsync()
        {
            await SynchronizeAsync(async () =>
            {
                if (!_started)
                {
                    await StartAsync();
                }
                else
                {
                    await ApplyOrdersAsync();
                }
            });
        }

        public IReadOnlyList<LimitOrder> GetBaseOrders()
        {
            return _baseAssetPairId != null && _ordersByAssetPairs.TryGetValue(_baseAssetPairId, out var orderBook)
                ? orderBook.LimitOrders
                : new List<LimitOrder>();
        }

        public IReadOnlyList<DependentLimitOrder> GetDependentOrders(string assetPairId)
        {
            return _ordersByAssetPairs.TryGetValue(assetPairId, out var orderBook)
                ? orderBook.LimitOrders.OfType<DependentLimitOrder>().ToList()
                : new List<DependentLimitOrder>();
        }
        
        private async Task HandleTradeAsync(IReadOnlyList<Trade> trades)
        {
            _log.Info("Trades are received", context: $"Trades: [{string.Join(", ", trades.Select( x=> x.ToJson()))}]");

            if (!_levelsService.GetLevels().Any())
            {
                _log.Error("Trade is received but there aren't any levels");
                return;
            }

            decimal volume = trades.Select(x => x.Type == TradeType.Sell ? -x.Volume : x.Volume).Sum();
            
            while (volume != 0)
            {
                volume = await HandleVolumeAsync(volume);
            }
            
            await _levelsService.SaveStatesAsync();
        }
        
        private async Task<decimal> HandleVolumeAsync(decimal volume)
        {
            if (volume < 0)
            {
                var level = _levelsService.GetLevels().OrderBy(e => e.Sell).First();

                volume = level.HandleSellVolume(volume);
                await UpdateInitialPrice(level.Reference);
                
                _log.Info($"Level {level.Name} is executed", context: $"Level state: {level.ToJson()}");

                return volume;
            }

            if (volume > 0)
            {
                var level = _levelsService.GetLevels().OrderByDescending(e => e.Buy).First();

                volume = level.HandleBuyVolume(volume);
                await UpdateInitialPrice(level.Reference);
                
                _log.Info($"Level {level.Name} is executed", context: $"Level state: {level.ToJson()}");
                
                return volume;
            }

            return 0;
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

        private async Task ApplyOrdersAsync()
        {
            try
            {
                if (await BaseAssetPairWasDeleted())
                {
                    _log.Info("Base pair deletion is detected, remove all orders");
                    _ordersByAssetPairs[_baseAssetPairId] = new OrderBook();
                    _started = false;
                }
                else
                {
                    var levelOrders = _levelsService.GetOrders().ToList();
                    var additionalOrders = await _additionalVolumeService.GetOrdersAsync(levelOrders);


                    var newBaseOrders = levelOrders.Union(additionalOrders).ToList();
                    if (_ordersByAssetPairs.ContainsKey(_baseAssetPairId) && newBaseOrders.SequenceEqual(_ordersByAssetPairs[_baseAssetPairId].LimitOrders))
                    {
                        _ordersByAssetPairs[_baseAssetPairId].IsChanged = false;
                    }
                    else
                    {
                        _ordersByAssetPairs[_baseAssetPairId] = new OrderBook(newBaseOrders);
                        _ordersByAssetPairs[_baseAssetPairId].LimitOrders.ForEach(x => x.AssetPairId = _baseAssetPairId); // TODO: fill in Levels?    
                    }
                }
                
                var dependentPairsSettings = (await _settingsService.GetDependentAssetPairsSettingsAsync()).ToList();
                foreach (var pairSettings in dependentPairsSettings)
                {
                    _ordersByAssetPairs[pairSettings.AssetPairId] =
                        new OrderBook(_ordersByAssetPairs[_baseAssetPairId].LimitOrders
                            .Select(x =>
                                (LimitOrder) _ordersConverter.ConvertAsync(x, pairSettings).GetAwaiter().GetResult())
                            .ToList());
                }

                ClearDeletedDependentPairs(dependentPairsSettings); // TODO: move to SettingsService

                foreach (var ordersByAssetPair in _ordersByAssetPairs.Where(x => x.Value.IsChanged))
                {
                    try
                    {
                        await _lykkeExchange.ApplyAsync(ordersByAssetPair.Key, ordersByAssetPair.Value.LimitOrders);
                    }
                    catch (Exception e)
                    {
                        _log.Error(e, $"Error on placing orders for {ordersByAssetPair.Key}");
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
        }

        private async Task<bool> BaseAssetPairWasDeleted()
        {
            return await _settingsService.GetBaseAssetPairSettingsAsync() == null;
        }

        private void ClearDeletedDependentPairs(IEnumerable<AssetPairSettings> dependentPairsSettings)
        {
            var allEnabledDependentAssetPairs = dependentPairsSettings.Select(x => x.AssetPairId);
            var deletedDependentAssetPairs =
                _ordersByAssetPairs.Keys.Where(x => !allEnabledDependentAssetPairs.Contains(x) && x != _baseAssetPairId);
            
            foreach (var dependentAssetPairId in deletedDependentAssetPairs)
            {
                _ordersByAssetPairs[dependentAssetPairId] = new OrderBook();
            }
        }
    }
}

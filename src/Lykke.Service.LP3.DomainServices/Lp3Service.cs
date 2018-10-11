using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.Assets.Client.Models.v3;
using Lykke.Service.Assets.Client.ReadModels;
using Lykke.Service.LP3.Domain;
using Lykke.Service.LP3.Domain.Exchanges;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Services;

namespace Lykke.Service.LP3.DomainServices
{
    public class Lp3Service : ILp3Service, IStartable
    {
        private readonly ISettingsService _settingsService;
        private readonly ILevelsService _levelsService;
        private readonly IAdditionalVolumeService _additionalVolumeService;
        private readonly IInitialPriceService _initialPriceService;
        private readonly ILykkeExchange _lykkeExchange;
        private readonly IAssetPairsReadModelRepository _assetsService;
        private readonly IOrdersConverter _ordersConverter;
        private readonly ILog _log;
        
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private bool _started;

        private List<LimitOrder> _orders = new List<LimitOrder>();
        private AssetPair _assetPair;
        
        private decimal _inventory = 0;
        private decimal _oppositeInventory = 0;

        public Lp3Service(ILogFactory logFactory,
            ISettingsService settingsService,
            ILevelsService levelsService,
            IAdditionalVolumeService additionalVolumeService,
            IInitialPriceService initialPriceService,
            ILykkeExchange lykkeExchange,
            IAssetPairsReadModelRepository assetsService,
            IOrdersConverter ordersConverter)
        {
            _settingsService = settingsService;
            _levelsService = levelsService;
            _additionalVolumeService = additionalVolumeService;
            _initialPriceService = initialPriceService;
            _lykkeExchange = lykkeExchange;
            _assetsService = assetsService;
            _ordersConverter = ordersConverter;
            _log = logFactory.CreateLog(this);
        }
        

        public void Start()
        {
            SynchronizeAsync(async () => await StartAsync()).GetAwaiter().GetResult();;
        }

        private async Task StartAsync()
        {
            var initialPrice = await _initialPriceService.GetAsync();
            if (initialPrice == null)
            {
                _log.Info("No initial price to start algorithm, waiting for adding one via API");
                return;
            }
        
            var baseAssetPairId = (await _settingsService.GetBaseAssetPairSettings())?.AssetPairId;
            if (baseAssetPairId == null)
            {
                _log.Info("No baseAssetPairId to start algorithm, waiting for adding it via API");
                return;
            }

            _assetPair = _assetsService.TryGet(baseAssetPairId);
            if (_assetPair == null)
            {
                _log.Error($"AssetService returned null for {baseAssetPairId}");
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
                var newInitialPrice = trades.Last().Price;
                await _initialPriceService.AddOrUpdateAsync(newInitialPrice);
                _log.Info("InitialPrice is updated", 
                    context: $"Trades: [{string.Join(", ", trades.Select(x => x.ToJson()))}], new InitialPrice: {newInitialPrice}");
                
                foreach (var trade in trades)
                {
                    HandleTrade(trade); // TODO: pass all trades at once?
                }

                await ApplyOrdersAsync();
            });
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

        public IReadOnlyList<LimitOrder> GetOrders()
        {
            return _orders;
        }
        
        private void HandleTrade(Trade trade)
        {
            _log.Info("Trade is received", context: $"Trade: {trade.ToJson()}");

            if (!_levelsService.GetLevels().Any())
            {
                _log.Error("Trade is received but there aren't any levels");
                return;
            }

            var volume = trade.Volume;

            if (trade.Type == TradeType.Sell)
            {
                volume *= -1;
            }
            
            while (volume != 0)
            {
                volume = HandleVolume(volume);
            }
            
            _levelsService.SaveStatesAsync().GetAwaiter().GetResult();
        }
        
        private decimal HandleVolume(decimal volume)
        {
            if (volume < 0)
            {
                var level = _levelsService.GetLevels().OrderBy(e => e.Sell).First();

                if (volume <= level.VolumeSell)
                {
                    volume -= level.VolumeSell;

                    _inventory += level.VolumeSell;
                    _oppositeInventory -= level.VolumeSell * level.Sell; // TODO: get rounded price from trade ? 

                    level.Inventory += level.VolumeSell;
                    level.OppositeInventory -= level.VolumeSell * level.Sell;

                    level.UpdateReference(level.Sell);
                    level.VolumeSell = -level.OriginalVolume;
                }
                else
                {
                    level.VolumeSell -= volume;

                    _inventory += volume;
                    _oppositeInventory -= volume * level.Sell;
                    level.Inventory += volume;
                    level.OppositeInventory -= volume * level.Sell;

                    volume = 0;
                }
                
                _log.Info($"Level {level.Name} is executed", context: $"Level state: {level.ToJson()}");

                return volume;
            }

            if (volume > 0)
            {
                var level = _levelsService.GetLevels().OrderByDescending(e => e.Buy).First();

                if (volume >= level.VolumeBuy)
                {
                    volume -= level.VolumeBuy;

                    _inventory += level.VolumeBuy;
                    _oppositeInventory -= level.VolumeBuy * level.Buy;

                    level.Inventory += level.VolumeBuy;
                    level.OppositeInventory -= level.VolumeBuy * level.Buy;

                    level.UpdateReference(level.Buy);
                    level.VolumeBuy = level.OriginalVolume;
                }
                else
                {
                    level.VolumeBuy -= volume;
                    _inventory += volume;
                    _oppositeInventory -= volume * level.Buy;
                    level.Inventory += volume;
                    level.OppositeInventory -= volume * level.Buy;
                    volume = 0;
                }
                
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
                var levelOrders = _levelsService.GetOrders().ToList();
                var additionalOrders = await _additionalVolumeService.GetOrdersAsync(levelOrders);

                _orders = levelOrders.Union(additionalOrders).ToList();

                await _lykkeExchange.ApplyAsync(_assetPair, _orders);

//                var dependentPairs = await _settingsService.GetDependentAssetPairsSettingsAsync();
//                foreach (var pair in dependentPairs)
//                {
//                    var ordersForPair = _orders.Select(x => _ordersConverter.Convert(x, pair)).ToList();
//                    var assetPair = _assetsService.TryGet(pair.AssetPairId);
//                    await _lykkeExchange.ApplyAsync(assetPair, ordersForPair);
//                }
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
        }
    }
}

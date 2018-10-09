using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.Assets.Client.Models.v3;
using Lykke.Service.Assets.Client.ReadModels;
using Lykke.Service.LP3.Domain;
using Lykke.Service.LP3.Domain.Exchanges;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Services;
using Lykke.Service.LP3.DomainServices.Extensions;

namespace Lykke.Service.LP3.DomainServices
{
    public class Lp3Service : ILp3Service, IStartable
    {
        private readonly ISettingsService _settingsService;
        private readonly ITradingAlgorithm _tradingAlgorithm;
        private readonly IInitialPriceService _initialPriceService;
        private readonly ILykkeExchange _lykkeExchange;
        private readonly IAssetPairsReadModelRepository _assetsService;
        private readonly ILog _log;
        
        private string _baseAssetPairId;
        
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private bool _started;

        private List<LimitOrder> _orders = new List<LimitOrder>();
        private AssetPair _assetPair;

        public Lp3Service(ILogFactory logFactory,
            ISettingsService settingsService,
            ITradingAlgorithm tradingAlgorithm,
            IInitialPriceService initialPriceService,
            ILykkeExchange lykkeExchange,
            IAssetPairsReadModelRepository assetsService)
        {
            _settingsService = settingsService;
            _tradingAlgorithm = tradingAlgorithm;
            _initialPriceService = initialPriceService;
            _lykkeExchange = lykkeExchange;
            _assetsService = assetsService;
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
        
            _baseAssetPairId = (await _settingsService.GetBaseAssetPairSettings())?.AssetPairId;
            if (_baseAssetPairId == null)
            {
                _log.Info("No baseAssetPairId to start algorithm, waiting for adding it via API");
                return;
            }

            _assetPair = _assetsService.TryGet(_baseAssetPairId);
            if (_assetPair == null)
            {
                _log.Error($"AssetService returned null for {_baseAssetPairId}");
                return;
            }

            await _tradingAlgorithm.StartAsync(initialPrice.Price);

            _started = true;
            
            await ApplyOrdersAsync();
        }

        public async Task HandleTradesAsync(IReadOnlyList<Trade> trades)
        {
            await SynchronizeAsync(async () =>
            {
                await _initialPriceService.AddOrUpdateAsync(trades.Last().Price);
                
                foreach (var trade in trades)
                {
                    _tradingAlgorithm.HandleTrade(trade); // TODO: pass all trades at once?
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

        public IReadOnlyList<Level> GetLevels()
        {
            return _tradingAlgorithm.GetLevels();
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
                _orders = _tradingAlgorithm.GetOrders().ToList();

                await _lykkeExchange.ApplyAsync(_assetPair, _orders);
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
        }
    }
}

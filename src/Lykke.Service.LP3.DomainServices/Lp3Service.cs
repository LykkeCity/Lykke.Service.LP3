using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.LP3.Domain;
using Lykke.Service.LP3.Domain.Exchanges;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Services;

namespace Lykke.Service.LP3.DomainServices
{
    public class Lp3Service : ILp3Service, IStartable
    {
        private readonly ISettingsService _settingsService;
        private readonly ITradingAlgorithm _tradingAlgorithm;
        private readonly IInitialPriceService _initialPriceService;
        private readonly ILykkeExchange _lykkeExchange;
        private readonly ILog _log;
        
        private string _baseAssetPairId;
        
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public Lp3Service(ILogFactory logFactory,
            ISettingsService settingsService,
            ITradingAlgorithm tradingAlgorithm,
            IInitialPriceService initialPriceService,
            ILykkeExchange lykkeExchange)
        {
            _settingsService = settingsService;
            _tradingAlgorithm = tradingAlgorithm;
            _initialPriceService = initialPriceService;
            _lykkeExchange = lykkeExchange;
            _log = logFactory.CreateLog(this);
        }
        

        public void Start()
        {
            StartAsync().GetAwaiter().GetResult();;
        }

        private async Task StartAsync()
        {
            await SynchronizeAsync(async () =>
            {
                var initialPrice = await _initialPriceService.GetAsync();
                if (initialPrice == null)
                {
                    _log.Info("No initial price to start algorithm, waiting for adding one via API");
                    return;
                }
            
                _baseAssetPairId = (await _settingsService.GetBaseAssetPairSettings())?.AssetPairId;

                await _tradingAlgorithm.StartAsync(initialPrice.Price);
                await ApplyOrdersAsync();    
            });
        }

        public async Task HandleTradesAsync(IReadOnlyList<Trade> trades)
        {
            await SynchronizeAsync(async () =>
            {
                foreach (var trade in trades)
                {
                    _tradingAlgorithm.HandleTrade(trade); // TODO: pass all trades at once?
                }

                await ApplyOrdersAsync();
            });
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
                var orders = _tradingAlgorithm.GetOrders().ToList();
                await _lykkeExchange.ApplyAsync(_baseAssetPairId, orders);
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
        }
    }
}

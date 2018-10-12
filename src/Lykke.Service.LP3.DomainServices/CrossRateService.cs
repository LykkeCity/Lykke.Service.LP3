using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.LP3.Domain;
using Lykke.Service.LP3.Domain.Repositories;
using Lykke.Service.LP3.Domain.Services;

namespace Lykke.Service.LP3.DomainServices
{
    public class CrossRateService : ICrossRateService
    {
        private readonly ILastTickPriceRepository _lastTickPriceRepository;
        private readonly ISettingsService _settingsService;

        public CrossRateService(ILastTickPriceRepository lastTickPriceRepository,
            ISettingsService settingsService)
        {
            _lastTickPriceRepository = lastTickPriceRepository;
            _settingsService = settingsService;
        }

        public async Task HandleAsync(TickPrice tickPrice)
        {
            var dependentPairs = await _settingsService.GetDependentAssetPairsSettingsAsync();

            if (dependentPairs.Any(x =>
                string.Equals(x.CrossInstrumentAssetPair, tickPrice.AssetPair, StringComparison.InvariantCultureIgnoreCase) &&
                string.Equals(x.CrossInstrumentSource, tickPrice.Source, StringComparison.InvariantCultureIgnoreCase)))
            {
                await _lastTickPriceRepository.AddOrUpdateAsync(tickPrice);    
            }
        }

        public async Task<TickPrice> GetLastTickPriceAsync(string source, string assetPair)
        {
            var tickPrices = await _lastTickPriceRepository.GetAllAsync();

            return tickPrices.SingleOrDefault(x =>
                string.Equals(x.Source, source, StringComparison.InvariantCultureIgnoreCase) &&
                string.Equals(x.AssetPair, assetPair, StringComparison.InvariantCultureIgnoreCase));

        }

        public Task<IReadOnlyList<TickPrice>> GetAllTickPricesAsync()
        {
            return _lastTickPriceRepository.GetAllAsync();
        }
    }
}

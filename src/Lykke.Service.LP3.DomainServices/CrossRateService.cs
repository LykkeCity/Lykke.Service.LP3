using System;
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
        private readonly ICrossInstrumentService _crossInstrumentService;

        public CrossRateService(ILastTickPriceRepository lastTickPriceRepository,
            ICrossInstrumentService crossInstrumentService)
        {
            _lastTickPriceRepository = lastTickPriceRepository;
            _crossInstrumentService = crossInstrumentService;
        }

        public async Task HandleAsync(TickPrice tickPrice)
        {
            var crossInstruments = await _crossInstrumentService.GetAllAsync();

            if (crossInstruments.Any(x =>
                string.Equals(x.AssetPairId, tickPrice.AssetPair, StringComparison.InvariantCultureIgnoreCase)
                && string.Equals(x.Exchange, tickPrice.Source, StringComparison.InvariantCultureIgnoreCase)))
            {
                await _lastTickPriceRepository.AddOrUpdateAsync(tickPrice);    
            }
        }
    }
}

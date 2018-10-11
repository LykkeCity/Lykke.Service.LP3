using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Domain.CrossInstruments;
using Lykke.Service.LP3.Domain.Repositories;
using Lykke.Service.LP3.Domain.Services;

namespace Lykke.Service.LP3.DomainServices
{
    public class CrossInstrumentService : ICrossInstrumentService
    {
        private readonly ICrossInstrumentRepository _crossInstrumentRepository;

        public CrossInstrumentService(ICrossInstrumentRepository crossInstrumentRepository)
        {
            _crossInstrumentRepository = crossInstrumentRepository;
        }

        public Task<IReadOnlyList<CrossInstrument>> GetAllAsync()
        {
            return _crossInstrumentRepository.GetAsync(); // TODO: add Cache
        }

        public Task<CrossInstrument> GetAsync(string exchange, string assetPairId)
        {
            return _crossInstrumentRepository.GetAsync(exchange, assetPairId);
        }

        public Task AddAsync(CrossInstrument crossInstrument)
        {
            return _crossInstrumentRepository.AddAsync(crossInstrument);
        }

        public Task UpdateAsync(CrossInstrument crossInstrument)
        {
            return _crossInstrumentRepository.UpdateAsync(crossInstrument);
        }

        public Task DeleteAsync(string exchange, string assetPairId)
        {
            return _crossInstrumentRepository.DeleteAsync(exchange, assetPairId);
        }
    }
}

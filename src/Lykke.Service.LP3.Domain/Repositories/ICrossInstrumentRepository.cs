using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Domain.CrossInstruments;

namespace Lykke.Service.LP3.Domain.Repositories
{
    public interface ICrossInstrumentRepository
    {
        Task<IReadOnlyList<CrossInstrument>> GetAsync();
        Task AddAsync(CrossInstrument crossInstrument);
        Task UpdateAsync(CrossInstrument crossInstrument);
        Task DeleteAsync(string exchange, string assetPairId);
        Task<CrossInstrument> GetAsync(string exchange, string assetPairId);
    }
}

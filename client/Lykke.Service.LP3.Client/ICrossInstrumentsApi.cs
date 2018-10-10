using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Client.Models.CrossInstruments;
using Refit;

namespace Lykke.Service.LP3.Client
{
    public interface ICrossInstrumentsApi
    {
        [Get("/api/crossInstruments")]
        Task<IReadOnlyList<CrossInstrumentModel>> GetAsync();
        
        [Post("/api/crossInstruments")]
        Task AddAsync(CrossInstrumentModel model);

        [Put("/api/crossInstruments")]
        Task UpdateAsync(CrossInstrumentModel model);

        [Delete("/api/crossInstruments")]
        Task DeleteAsync(string exchange, string assetPairId);
    }
}

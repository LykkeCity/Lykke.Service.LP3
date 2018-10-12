using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.LP3.Domain.Services
{
    public interface ICrossRateService
    {
        Task HandleAsync(TickPrice tickPrice);
        
        Task<TickPrice> GetLastTickPriceAsync(string source, string assetPair);

        Task<IReadOnlyList<TickPrice>> GetAllTickPricesAsync();
    }
}

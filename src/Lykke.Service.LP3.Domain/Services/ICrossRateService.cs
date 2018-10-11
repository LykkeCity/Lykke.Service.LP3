using System.Threading.Tasks;

namespace Lykke.Service.LP3.Domain.Services
{
    public interface ICrossRateService
    {
        Task HandleAsync(TickPrice tickPrice);
        Task<TickPrice> GetLastTickPriceAsync(string source, string assetPair);
    }
}

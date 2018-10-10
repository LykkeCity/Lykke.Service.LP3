using System.Threading.Tasks;
using Lykke.MatchingEngine.ExchangeModels;

namespace Lykke.Service.LP3.Domain.Services
{
    public interface ICrossRateService
    {
        Task HandleAsync(string exchange, OrderBook orderBook);
    }
}

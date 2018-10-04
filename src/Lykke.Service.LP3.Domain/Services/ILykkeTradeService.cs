using System.Threading.Tasks;
using Lykke.MatchingEngine.ExchangeModels;

namespace Lykke.Service.LP3.Domain.Services
{
    public interface ILykkeTradeService
    {
        Task HandleAsync(LimitOrders limitOrders);
    }
}

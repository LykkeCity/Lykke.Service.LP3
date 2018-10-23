using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Domain.Orders;

namespace Lykke.Service.LP3.Domain.Exchanges
{
    public interface ILykkeExchange
    {
        Task ApplyAsync(string assetPairId, IReadOnlyCollection<LimitOrder> limitOrders);
        Task PlaceLimitOrderAsync(LimitOrder limitOrder);
        Task CancelLimitOrderAsync(string id);
    }
}

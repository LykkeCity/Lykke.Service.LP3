using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Domain.Orders;

namespace Lykke.Service.LP3.Domain.Exchanges
{
    public interface ILykkeExchange
    {
        Task ApplyAsync(Instrument instrument, IReadOnlyList<LimitOrder> limitOrders);
    }
}

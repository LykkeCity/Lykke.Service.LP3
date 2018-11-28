using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Domain.Orders;

namespace Lykke.Service.LP3.Domain.Services
{
    public interface ILimitOrderStore
    {
        Task MarkOrdersDisabled(bool isDisabled);
        Task ClearAndAddOrders(IEnumerable<LimitOrder> orders);
        Task<IReadOnlyCollection<LimitOrder>> GetOrders(TradeType? tradeType = default(TradeType?));
        Task AddSingleOrder(LimitOrder limitOrder);
        Task<LimitOrder> RemoveSingleOrder(Guid id);
        Task Clear();
        Task PersistOrder(LimitOrder order);
    }
}

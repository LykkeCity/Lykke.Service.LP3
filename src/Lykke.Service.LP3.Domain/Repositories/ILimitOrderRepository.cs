using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Domain.Orders;

namespace Lykke.Service.LP3.Domain.Repositories
{
    public interface ILimitOrderRepository
    {
        Task AddAsync(LimitOrder order);
        Task<IReadOnlyList<LimitOrder>> GetAllAsync();
        Task UpdateAsync(LimitOrder limitOrder);
        Task DeleteAsync(Guid orderId);
        Task ClearAsync();
        Task AddBatchAsync(LinkedList<LimitOrder> orders);
    }
}

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
        Task DeleteAsync(string assetPairId, Guid orderId);
        Task ClearAsync(string assetPairId);
        Task AddOrUpdateBatchAsync(IEnumerable<LimitOrder> orders);
    }
}

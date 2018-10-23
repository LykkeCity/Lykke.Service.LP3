using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Domain.Orders;

namespace Lykke.Service.LP3.Domain.Services
{
    public interface ILimitOrderService
    {
        Task AddAsync(LimitOrder order);
        
        Task<IReadOnlyList<LimitOrder>> GetAllAsync();
        
        Task UpdateAsync(LimitOrder limitOrder);
        
        Task DeleteAsync(string assetPairId, Guid orderId);
        
        Task ClearAsync(string assetPairId);
        
        Task AddBatchAsync(IEnumerable<LimitOrder> orders);
        
        Task UpdateBatchAsync(IEnumerable<LimitOrder> orders);
    }
}

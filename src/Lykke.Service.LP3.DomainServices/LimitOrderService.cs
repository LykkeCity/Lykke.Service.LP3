using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Repositories;
using Lykke.Service.LP3.Domain.Services;

namespace Lykke.Service.LP3.DomainServices
{
    public class LimitOrderService : ILimitOrderService
    {
        private readonly ILimitOrderRepository _limitOrderRepository;

        public LimitOrderService(ILimitOrderRepository limitOrderRepository)
        {
            _limitOrderRepository = limitOrderRepository;
        }

        public Task AddAsync(LimitOrder order)
        {
            return _limitOrderRepository.AddAsync(order);
        }

        public Task<IReadOnlyList<LimitOrder>> GetAllAsync()
        {
            return _limitOrderRepository.GetAllAsync();
        }

        public Task UpdateAsync(LimitOrder limitOrder)
        {
            return _limitOrderRepository.UpdateAsync(limitOrder);
        }

        public Task DeleteAsync(string assetPairId, Guid orderId)
        {
            return _limitOrderRepository.DeleteAsync(assetPairId, orderId);
        }

        public Task ClearAsync(string assetPairId)
        {
            return _limitOrderRepository.ClearAsync(assetPairId);
        }

        public Task AddBatchAsync(IEnumerable<LimitOrder> orders)
        {
            return _limitOrderRepository.AddBatchAsync(orders);
        }

        public Task UpdateBatchAsync(IEnumerable<LimitOrder> orders)
        {
            return _limitOrderRepository.UpdateBatchAsync(orders);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Repositories;

namespace Lykke.Service.LP3.AzureRepositories
{
    public class LimitOrderRepository : ILimitOrderRepository
    {
        private readonly INoSQLTableStorage<LimitOrderEntity> _storage;

        public LimitOrderRepository(INoSQLTableStorage<LimitOrderEntity> storage)
        {
            _storage = storage;
        }

        public async Task AddAsync(LimitOrder order)
        {
            var entity = new LimitOrderEntity(GetPartitionKey(), GetRowKey(order.Id));

            Mapper.Map(order, entity);

            await _storage.InsertAsync(entity);
        }
        
        public async Task<IReadOnlyList<LimitOrder>> GetAllAsync()
        {
            IEnumerable<LimitOrderEntity> data = await _storage.GetDataAsync(GetPartitionKey());

            return Mapper.Map<List<LimitOrder>>(data);
        }

        public async Task UpdateAsync(LimitOrder limitOrder)
        {
            var entity = new LimitOrderEntity(GetPartitionKey(), GetRowKey(limitOrder.Id));

            Mapper.Map(limitOrder, entity);

            await _storage.InsertOrMergeAsync(entity);
        }

        public async Task DeleteAsync(Guid orderId)
        {
            await _storage.DeleteAsync(GetPartitionKey(), GetRowKey(orderId));
        }

        public async Task ClearAsync()
        {
            await _storage.DeleteAsync();
        }

        public async Task AddBatchAsync(LinkedList<LimitOrder> orders)
        {
            var models = orders.Select(x =>
            {
                var entity = new LimitOrderEntity(GetPartitionKey(), GetRowKey(x.Id));
                Mapper.Map(x, entity);
                return entity;
            });

            await _storage.InsertOrMergeBatchAsync(models);
        }

        private string GetPartitionKey() => "LimitOrder";

        private string GetRowKey(Guid orderId) => orderId.ToString("N");
    }
}

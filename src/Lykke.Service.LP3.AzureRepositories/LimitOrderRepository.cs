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
            var entity = new LimitOrderEntity(GetPartitionKey(order.AssetPairId), GetRowKey(order.Id));

            Mapper.Map(order, entity);

            await _storage.InsertAsync(entity);
        }
        
        public async Task<IReadOnlyList<LimitOrder>> GetAllAsync()
        {
            IEnumerable<LimitOrderEntity> data = await _storage.GetDataAsync();

            return Mapper.Map<List<LimitOrder>>(data);
        }
        
        public async Task<IReadOnlyList<LimitOrder>> GetAllForAssetPairAsync(string assetPairId)
        {
            IEnumerable<LimitOrderEntity> data = await _storage.GetDataAsync(GetPartitionKey(assetPairId));

            return Mapper.Map<List<LimitOrder>>(data);
        }

        public async Task UpdateAsync(LimitOrder limitOrder)
        {
            var entity = new LimitOrderEntity(GetPartitionKey(limitOrder.AssetPairId), GetRowKey(limitOrder.Id));

            Mapper.Map(limitOrder, entity);

            await _storage.InsertOrMergeAsync(entity);
        }

        public async Task DeleteAsync(string assetPairId, Guid orderId)
        {
            await _storage.DeleteAsync(GetPartitionKey(assetPairId), GetRowKey(orderId));
        }

        public async Task ClearAsync(string assetPairId)
        {
            var data = await _storage.GetDataAsync(GetPartitionKey(assetPairId));
            await _storage.DeleteAsync(data);
        }

        public async Task AddBatchAsync(IEnumerable<LimitOrder> orders)
        {
            var models = orders.Select(x =>
            {
                var entity = new LimitOrderEntity(GetPartitionKey(x.AssetPairId), GetRowKey(x.Id));
                Mapper.Map(x, entity);
                return entity;
            });

            await _storage.InsertOrMergeBatchAsync(models);
        }

        private string GetPartitionKey(string assetPairId) => assetPairId.ToUpperInvariant();

        private string GetRowKey(Guid orderId) => orderId.ToString("N");
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.LP3.Domain.Repositories;

namespace Lykke.Service.LP3.AzureRepositories
{
    public class OrderIdsMappingRepository : IOrderIdsMappingRepository
    {
        private readonly INoSQLTableStorage<OrderIdsMappingEntity> _storage;

        public OrderIdsMappingRepository(INoSQLTableStorage<OrderIdsMappingEntity> storage)
        {
            _storage = storage;
        }
        
        public async Task PersistMapping(string assetPairId, Dictionary<Guid, string> mapping)
        {
            var entities = new List<OrderIdsMappingEntity>();
            
            foreach (var keyValue in mapping)
            {
                entities.Add(new OrderIdsMappingEntity(GetPartitionKey(assetPairId), GetRowKey(keyValue.Key))
                {
                    AssetPairId = assetPairId,
                    InternalId = keyValue.Key,
                    MultiOrderItemId = keyValue.Value
                });
            }

            await ClearPartition(GetPartitionKey(assetPairId));
            
            await _storage.InsertOrReplaceBatchAsync(entities);
        }

        private async Task ClearPartition(string partitionKey)
        {
            var entities = await _storage.GetDataAsync(partitionKey);
            await _storage.DeleteAsync(entities);
        }

        public async Task<Dictionary<string, Dictionary<Guid, string>>> RestoreMapping()
        {
            var data = await _storage.GetDataAsync();

            return data.GroupBy(x => x.AssetPairId).ToDictionary(x => x.Key,
                x => x.ToDictionary(e => e.InternalId, e => e.MultiOrderItemId));
        }

        public async Task DeleteMappingsAsync()
        {
            await _storage.DeleteAsync();
        }

        private string GetPartitionKey(string assetPairId) => assetPairId.ToUpperInvariant();

        private string GetRowKey(Guid id) => id.ToString("N");
    }
}

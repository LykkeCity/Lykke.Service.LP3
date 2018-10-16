using System;
using Lykke.AzureStorage.Tables;

namespace Lykke.Service.LP3.AzureRepositories
{
    public class OrderIdsMappingEntity : AzureTableEntity
    {
        public OrderIdsMappingEntity()
        {
        }

        public OrderIdsMappingEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        public string AssetPairId { get; set; }
        
        public Guid InternalId { get; set; }
        
        public string MultiOrderItemId { get; set; }
    }
}

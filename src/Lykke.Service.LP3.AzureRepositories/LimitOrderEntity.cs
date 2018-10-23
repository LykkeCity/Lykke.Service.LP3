using System;
using Lykke.AzureStorage.Tables;
using Lykke.Service.LP3.Domain.Orders;

namespace Lykke.Service.LP3.AzureRepositories
{
    public class LimitOrderEntity : AzureTableEntity
    {
        public LimitOrderEntity()
        {
            
        }

        public LimitOrderEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }
        
        public Guid Id { get; set; }
        
        public string ExternalId { get; set; }
        
        public decimal Number { get; set; }

        public decimal Price { get; set; }

        public decimal Volume { get; set; }
        
        public TradeType TradeType { get; set; }
        
        public LimitOrderError Error { get; set; }
        
        public string ErrorMessage { get; set; }
        
        public string AssetPairId { get; set; }
    }
}

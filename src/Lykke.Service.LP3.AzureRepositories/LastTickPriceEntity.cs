using System;
using Lykke.AzureStorage.Tables;

namespace Lykke.Service.LP3.AzureRepositories
{
    public class LastTickPriceEntity : AzureTableEntity
    {
        public LastTickPriceEntity()
        {
            
        }

        public LastTickPriceEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }
        
        public string Source { get; set; }
        
        public string AssetPair { get; set; }
        
        public DateTime DateTime { get; set; }
        
        public decimal Ask { get; set; }
        
        public decimal Bid { get; set; }
    }
}

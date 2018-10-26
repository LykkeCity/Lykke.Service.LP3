using Lykke.AzureStorage.Tables;

namespace Lykke.Service.LP3.AzureRepositories
{
    public class AssetPairEntity : AzureTableEntity
    {
        public AssetPairEntity()
        {
            
        }

        public AssetPairEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }
        
        public string AssetPairId { get; set; }
        
        public string Name { get; set; }
        
        public string ExternalId { get; set; }
        
        public int VolumeAccuracy { get; set; }
        
        public int PriceAccuracy { get; set; }
        
        public decimal MinVolume { get; set; }
        
        public string BaseAssetExternalId { get; set; }
        
        public string QuoteAssetExternalId { get; set; }
    }
}

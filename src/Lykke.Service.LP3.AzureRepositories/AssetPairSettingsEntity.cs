using Lykke.AzureStorage.Tables;

namespace Lykke.Service.LP3.AzureRepositories
{
    public class AssetPairSettingsEntity : AzureTableEntity
    {
        public AssetPairSettingsEntity()
        {
            
        }

        public AssetPairSettingsEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }
        
        public string AssetPairId { get; set; }
        
        public bool IsReversed { get; set; }
        
        public string CrossInstrumentSource { get; set; }
        
        public string CrossInstrumentAssetPair { get; set; }
        
        public bool IsCrossInstrumentReversed { get; set; }
    }
}

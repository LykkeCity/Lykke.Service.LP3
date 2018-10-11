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
    }
}

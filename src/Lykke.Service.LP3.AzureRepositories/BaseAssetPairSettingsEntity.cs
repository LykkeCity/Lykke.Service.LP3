using Lykke.AzureStorage.Tables;

namespace Lykke.Service.LP3.AzureRepositories
{
    public class BaseAssetPairSettingsEntity : AzureTableEntity
    {
        public BaseAssetPairSettingsEntity()
        {
            
        }

        public BaseAssetPairSettingsEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }
        
        public string AssetPairId { get; set; }
    }
}

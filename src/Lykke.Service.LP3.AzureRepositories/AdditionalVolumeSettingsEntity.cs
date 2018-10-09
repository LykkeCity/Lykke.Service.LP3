using Lykke.AzureStorage.Tables;

namespace Lykke.Service.LP3.AzureRepositories
{
    public class AdditionalVolumeSettingsEntity : AzureTableEntity
    {
        public AdditionalVolumeSettingsEntity()
        {
            
        }

        public AdditionalVolumeSettingsEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }
        
        public decimal Delta { get; set; }
        
        public int Count { get; set; }
        
        public decimal Volume { get; set; }
    }
}

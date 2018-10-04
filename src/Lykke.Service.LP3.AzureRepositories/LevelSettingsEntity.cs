using Lykke.AzureStorage.Tables;

namespace Lykke.Service.LP3.AzureRepositories
{
    public class LevelSettingsEntity : AzureTableEntity
    {
        public LevelSettingsEntity()
        {
            
        }

        public LevelSettingsEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }
        
        public string Name { get; set; }
        
        public decimal Delta { get; set; }
        
        public decimal Volume { get; set; }
    }
}

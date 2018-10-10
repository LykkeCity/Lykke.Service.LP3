using Lykke.AzureStorage.Tables;

namespace Lykke.Service.LP3.AzureRepositories
{
    public class CrossRateEntity : AzureTableEntity
    {
        public CrossRateEntity()
        {
            
        }

        public CrossRateEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }
    }
}

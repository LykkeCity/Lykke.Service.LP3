using Lykke.AzureStorage.Tables;

namespace Lykke.Service.LP3.AzureRepositories
{
    public class InitialPriceEntity : AzureTableEntity
    {
        public InitialPriceEntity()
        {
            
        }

        public InitialPriceEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }
        
        public decimal Price { get; set; }
    }
}

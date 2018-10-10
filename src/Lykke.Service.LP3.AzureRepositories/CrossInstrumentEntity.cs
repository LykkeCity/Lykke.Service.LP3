using JetBrains.Annotations;
using Lykke.AzureStorage.Tables;

namespace Lykke.Service.LP3.AzureRepositories
{
    [UsedImplicitly]
    public class CrossInstrumentEntity : AzureTableEntity
    {
        public CrossInstrumentEntity()
        {
            
        }

        public CrossInstrumentEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }
        
        public string Exchange { get; set; }
        
        public string AssetPairId { get; set; }
        
        public string BaseAsset { get; set; }
        
        public string QuoteAsset { get; set; }
    }
}

using System;
using JetBrains.Annotations;
using Lykke.AzureStorage.Tables;
using Lykke.Service.LP3.Domain.Orders;

namespace Lykke.Service.LP3.AzureRepositories
{
    [UsedImplicitly]
    public class TradeEntity : AzureTableEntity
    {
        public TradeEntity()
        {
        }

        public TradeEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        public string Id { get; set; }
        
        public string LimitOrderId { get; set; }

        public string ExchangeOrderId { get; set; }

        public string AssetPairId { get; set; }

        public string Type { get; set; }

        public DateTime Time { get; set; }

        public decimal Price { get; set; }

        public decimal Volume { get; set; }

        public decimal RemainingVolume { get; set; }

        public TradeStatus Status { get; set; }

        public decimal OppositeSideVolume { get; set; }

        public string ClientId { get; set; }
        
        public string OppositeClientId { get; set; }

        public string OppositeLimitOrderId { get; set; }
    }
}

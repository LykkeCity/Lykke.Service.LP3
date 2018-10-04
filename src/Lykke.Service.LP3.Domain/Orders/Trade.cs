using System;

namespace Lykke.Service.LP3.Domain.Orders
{
    public class Trade
    {
        public string Id { get; set; }

        public string LimitOrderId { get; set; }

        public string ExchangeOrderId { get; set; }

        public string AssetPairId { get; set; }

        public TradeType Type { get; set; }

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

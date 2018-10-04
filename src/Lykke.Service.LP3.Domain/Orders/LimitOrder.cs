using System;

namespace Lykke.Service.LP3.Domain.Orders
{
    public class LimitOrder
    {
        public Guid Id { get; }

        public decimal Price { get; }

        public decimal Volume { get;  }
        
        public TradeType TradeType { get; }
        
        public LimitOrderError Error { get; set; }
        
        public string ErrorMessage { get; set; }

        public LimitOrder(decimal price, decimal volume, TradeType tradeType)
        {
            Id = Guid.NewGuid();
            
            Price = price;
            Volume = volume;
            TradeType = tradeType;
        }
    }
}

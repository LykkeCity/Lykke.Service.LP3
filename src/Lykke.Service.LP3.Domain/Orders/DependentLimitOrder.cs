using System;

namespace Lykke.Service.LP3.Domain.Orders
{
    public class DependentLimitOrder : LimitOrder
    {
        public string Description { get; set; }
        
        public LimitOrder BaseLimitOrder { get; set; }
        
        public TickPrice CrossTickPrice { get; set; }

        public DependentLimitOrder(decimal price, decimal volume, TradeType tradeType) : base(price, volume, tradeType)
        {
        }

        public DependentLimitOrder(Guid id, decimal price, decimal volume, TradeType tradeType) : base(id, price, volume, tradeType)
        {
        }
    }
}

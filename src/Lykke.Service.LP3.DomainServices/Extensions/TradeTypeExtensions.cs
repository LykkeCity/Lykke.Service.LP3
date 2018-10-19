using System;
using Lykke.Service.LP3.Domain.Orders;

namespace Lykke.Service.LP3.DomainServices.Extensions
{
    public static class TradeTypeExtensions
    {
        public static TradeType Reverse(this TradeType tradeType)
        {
            switch (tradeType)
            {
                case TradeType.Buy:
                    return TradeType.Sell;
                case TradeType.Sell:
                    return TradeType.Buy;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tradeType), tradeType, null);
            }
        }
    }
}

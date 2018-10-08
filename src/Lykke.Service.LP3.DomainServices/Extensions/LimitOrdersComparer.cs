using System.Collections.Generic;
using Lykke.Service.LP3.Domain.Orders;

namespace Lykke.Service.LP3.DomainServices.Extensions
{
    public class LimitOrdersComparer : IEqualityComparer<LimitOrder>
    {
        public bool Equals(LimitOrder x, LimitOrder y)
        {
            return x.Price == y.Price &&
                   x.Volume == y.Volume &&
                   x.TradeType == y.TradeType;
        }

        public int GetHashCode(LimitOrder obj)
        {
            return obj.GetHashCode();
        }
    }
}

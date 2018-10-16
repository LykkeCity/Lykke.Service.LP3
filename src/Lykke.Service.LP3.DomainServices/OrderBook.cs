using System.Collections.Generic;
using System.Linq;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.DomainServices.Extensions;

namespace Lykke.Service.LP3.DomainServices
{
    public class OrderBook
    {
        public OrderBook()
        {
            LimitOrders = new List<LimitOrder>();
        }
        
        public OrderBook(IReadOnlyList<LimitOrder> limitOrders)
        {
            LimitOrders = limitOrders;
        }

        public IReadOnlyList<LimitOrder> LimitOrders { get; }

        public bool IsChanged { get; set; } = true;

        public bool Equal(OrderBook orderBook)
        {
            return LimitOrders.SequenceEqual(orderBook.LimitOrders, new LimitOrdersComparer());
        }
    }
}

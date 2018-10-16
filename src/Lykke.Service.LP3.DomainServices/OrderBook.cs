using System.Collections.Generic;
using Lykke.Service.LP3.Domain.Orders;

namespace Lykke.Service.LP3.DomainServices
{
    public class OrderBook
    {
        public OrderBook()
        {
            LimitOrders = new List<LimitOrder>();
        }
        
        public OrderBook(List<LimitOrder> limitOrders)
        {
            LimitOrders = limitOrders;
        }

        public List<LimitOrder> LimitOrders { get; }

        public bool IsChanged { get; set; } = true;
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Domain.Orders;

namespace Lykke.Service.LP3.Domain.Services
{
    public interface ITrader
    {
        Task StartAsync(decimal startMid);
        
        Task HandleTradeAsync(Trade trade);
        IEnumerable<LimitOrder> GetOrders();
    }
}

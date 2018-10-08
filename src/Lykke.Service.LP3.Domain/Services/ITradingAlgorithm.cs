using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Domain.Orders;

namespace Lykke.Service.LP3.Domain.Services
{
    public interface ITradingAlgorithm
    {
        Task StartAsync(decimal startMid);
        void HandleTrade(Trade trade);
        IEnumerable<LimitOrder> GetOrders();
        IReadOnlyList<Level> GetLevels();
    }
}

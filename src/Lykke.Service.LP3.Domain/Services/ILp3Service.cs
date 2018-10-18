using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Settings;

namespace Lykke.Service.LP3.Domain.Services
{
    public interface ILp3Service
    {
        Task HandleTradesAsync(IReadOnlyCollection<Trade> trades);
        
        IReadOnlyCollection<LimitOrder> GetOrders();
        
        Task UpdateOrderBookTraderSettingsAsync(OrderBookTraderSettings orderBookTraderSettings);
        
        Task AddOrderBookTraderAsync(OrderBookTraderSettings orderBookTraderSettings);
    }
}

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
        
        Task DeleteOrderBookAsync(string assetPairId);
        
        Task ForceReplaceOrderBookAsync(string assetPairId);
        
        Task AddOrderAsync(LimitOrder limitOrder);
        
        Task CancelOrderAsync(string orderId);
        
        Task CancelAllOrdersAsync(string assetPairId);
        
        Task<LimitOrder> RecreateOrderAsync(string orderId);
    }
}

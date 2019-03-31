using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Settings;

namespace Lykke.Service.LP3.Domain.Services
{
    public interface ILp3Service
    {
        Task ApplyOrderBooksAsync();
        
        Task HandleTradesAsync(IReadOnlyCollection<Trade> trades);
        
        Task<IReadOnlyCollection<LimitOrder>> GetAllOrdersAsync();
        
        Task<IReadOnlyCollection<LimitOrder>> GetOrdersForAssetAsync(string assetPairId);
        
        Task UpdateOrderBookTraderSettingsAsync(OrderBookTraderSettings orderBookTraderSettings);
        
        Task AddOrderBookTraderAsync(OrderBookTraderSettings orderBookTraderSettings);
        
        Task DeleteOrderBookAsync(string assetPairId);
        
        Task AddOrderAsync(LimitOrder limitOrder);
        
        Task CancelOrderAsync(string assetPairId, Guid orderId);
        
        Task CancelAllOrdersAsync(string assetPairId);
        
        Task<LimitOrder> RecreateOrderAsync(string assetPairId, Guid orderId);
        
        Task SoftStopAsync(string assetPairId);
        
        Task SoftStartAsync(string assetPairId);
        
        Task ForceReplaceOrderBookAsync(string assetPairId);
    }
}

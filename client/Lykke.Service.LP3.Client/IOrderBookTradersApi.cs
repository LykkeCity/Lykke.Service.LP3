using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Client.Models;
using Lykke.Service.LP3.Client.Models.Settings;
using Refit;

namespace Lykke.Service.LP3.Client
{
    public interface IOrderBookTradersApi
    {
        [Get("/api/orderBookTraders")]
        Task<IReadOnlyList<OrderBookTraderModel>> GetAllAsync();

        [Get("/api/orderBookTraders/{assetPairId}")]
        Task<OrderBookTraderModel> GetAsync(string assetPairId);

        [Put("/api/orderBookTraders")]
        Task UpdateOrderBookTraderSettingsAsync(OrderBookTraderSettingsModel model);
                
        [Post("/api/orderBookTraders")]
        Task AddOrderBookTraderAsync(OrderBookTraderSettingsModel model);
        
        [Delete("/api/orderBookTraders/{assetPairId}")]
        Task DeleteOrderBookTraderAsync(string assetPairId);
        
        [Post("/api/orderBookTraders/{assetPairId}/softStop")]
        Task SoftStopAsync(string assetPairId);

        [Post("/api/orderBookTraders/{assetPairId}/softStart")]
        Task SoftStartAsync(string assetPairId);

        [Post("/api/orderBookTraders/{assetPairId}/forceReplace")]
        Task ForceReplaceOrderBookAsync(string assetPairId);
    }
}

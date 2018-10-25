using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Client.Models;
using Lykke.Service.LP3.Client.Models.Settings;
using Refit;

namespace Lykke.Service.LP3.Client
{
    public interface IOrderBookTradersApi
    {
        /// <summary>
        /// Get all current traders
        /// </summary>
        [Get("/api/orderBookTraders")]
        Task<IReadOnlyList<OrderBookTraderModel>> GetAllAsync();

        /// <summary>
        /// Get a trader for particular order book by assetPair id 
        /// </summary>
        [Get("/api/orderBookTraders/{assetPairId}")]
        Task<OrderBookTraderModel> GetAsync(string assetPairId);

        /// <summary>
        /// Update settings of particular trader. It will reinit the trader with replacement of all its orders
        /// </summary>
        [Put("/api/orderBookTraders")]
        Task UpdateOrderBookTraderSettingsAsync(OrderBookTraderSettingsModel model);
                
        /// <summary>
        /// Add a trader for new order book
        /// </summary>
        [Post("/api/orderBookTraders")]
        Task AddOrderBookTraderAsync(OrderBookTraderSettingsModel model);
        
        /// <summary>
        /// Delete particular trader and cancel all its orders
        /// </summary>
        [Delete("/api/orderBookTraders/{assetPairId}")]
        Task DeleteOrderBookTraderAsync(string assetPairId);
        
        /// <summary>
        /// Disable particular trader, it will cancel all its orders but keep them in memory in disabled state
        /// </summary>
        [Post("/api/orderBookTraders/{assetPairId}/softStop")]
        Task SoftStopAsync(string assetPairId);

        /// <summary>
        /// If the trader is soft stopped, it will replace all its orders into exchange
        /// </summary>
        [Post("/api/orderBookTraders/{assetPairId}/softStart")]
        Task SoftStartAsync(string assetPairId);

        /// <summary>
        /// Cancels all trader's orders and place them into exchange again
        /// </summary>
        [Post("/api/orderBookTraders/{assetPairId}/forceReplace")]
        Task ForceReplaceOrderBookAsync(string assetPairId);
    }
}

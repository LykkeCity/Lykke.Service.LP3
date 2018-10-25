using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Client.Models.Orders;
using Refit;

namespace Lykke.Service.LP3.Client
{
    public interface IOrdersApi
    {
        /// <summary>
        /// Get current orders for all pairs
        /// </summary>
        [Get("/api/orders")]
        Task<IReadOnlyList<LimitOrderModel>> GetAllOrdersAsync();
        
        /// <summary>
        /// Get current orders for particular pair
        /// </summary>
        [Get("/api/orders/{assetPairId}")]
        Task<IReadOnlyList<LimitOrderModel>> GetOrdersAsync(string assetPairId);

        /// <summary>
        /// Manually create an order and add it to the trader of order's asset pair
        /// </summary>
        [Post("/api/orders")]
        Task CreateOrderAsync(LimitOrderPostModel orderModel);

        /// <summary>
        /// Cancel particular order
        /// </summary>
        [Delete("/api/orders/{assetPairId}/{orderId}")]
        Task CancelOrderAsync(string assetPairId, Guid orderId);

        /// <summary>
        /// Cancel particular order and recreate it on the exchange again
        /// </summary>
        [Post("/api/orders/{assetPairId}/{orderId}/recreate")]
        Task<LimitOrderModel> RecreateOrderAsync(string assetPairId, Guid orderId);

        /// <summary>
        /// Cancel all orders for particular trader, the trader keep staying in enabled mode
        /// </summary>
        [Delete("/api/orders/{assetPairId}")]
        Task CancelAllOrdersAsync(string assetPairId);
    }
}

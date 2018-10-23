using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Client.Models.Orders;
using Refit;

namespace Lykke.Service.LP3.Client
{
    public interface IOrdersApi
    {
        [Get("/api/orders")]
        Task<IReadOnlyList<LimitOrderModel>> GetAllOrdersAsync();
        
        [Get("/api/orders/{assetPairId}")]
        Task<IReadOnlyList<LimitOrderModel>> GetOrdersAsync(string assetPairId);

        [Post("/api/orders")]
        Task CreateOrderAsync(LimitOrderModel orderModel);

        [Delete("/api/orders/{assetPairId}/{orderId}")]
        Task CancelOrderAsync(string assetPairId, Guid orderId);

        [Post("/api/orders/{assetPairId}/{orderId}/recreate")]
        Task<LimitOrderModel> RecreateOrderAsync(string assetPairId, Guid orderId);
    }
}

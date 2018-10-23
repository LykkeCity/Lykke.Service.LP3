using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Client.Models.Orders;
using Refit;

namespace Lykke.Service.LP3.Client
{
    public interface IOrdersApi
    {
        [Get("/api/orders")]
        Task<IReadOnlyList<LimitOrderModel>> GetOrdersAsync(string assetPairId);

        [Post("/api/orders")]
        Task CreateOrderAsync(LimitOrderModel orderModel);

        [Delete("/api/orders/{orderId}")]
        Task CancelOrderAsync(string orderId);
    }
}

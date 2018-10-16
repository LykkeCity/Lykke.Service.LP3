using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Client.Models.Orders;
using Refit;

namespace Lykke.Service.LP3.Client
{
    public interface IOrdersApi
    {
        [Get("/api/orders")]
        Task<IReadOnlyList<LimitOrderModel>> GetBaseOrdersAsync();
        
        [Get("/api/orders/{assetPairId}")]
        Task<IReadOnlyList<DependentLimitOrderModel>> GetDependentOrdersAsync(string assetPairId);
    }
}

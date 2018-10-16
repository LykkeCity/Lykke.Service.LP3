using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Domain.Assets;
using Lykke.Service.LP3.Domain.Orders;

namespace Lykke.Service.LP3.Domain.Services
{
    public interface IAdditionalVolumeService
    {
        Task<IEnumerable<LimitOrder>> GetOrdersAsync(IEnumerable<LimitOrder> currentOrders,
            AssetPairInfo baseAssetPairInfo);
    }
}

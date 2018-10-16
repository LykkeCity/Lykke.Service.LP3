using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Settings;

namespace Lykke.Service.LP3.Domain.Services
{
    public interface IOrdersConverter
    {
        Task<IReadOnlyList<DependentLimitOrder>> ConvertAsync(IReadOnlyList<LimitOrder> orders, AssetPairSettings assetPairSettings);
    }
}

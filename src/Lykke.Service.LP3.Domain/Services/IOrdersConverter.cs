using System.Threading.Tasks;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Settings;

namespace Lykke.Service.LP3.Domain.Services
{
    public interface IOrdersConverter
    {
        Task<DependentLimitOrder> ConvertAsync(LimitOrder order, AssetPairSettings assetPairSettings);
    }
}

using System.Threading.Tasks;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Settings;

namespace Lykke.Service.LP3.Domain.Services
{
    public interface IOrdersConverter
    {
        Task<LimitOrder> ConvertAsync(LimitOrder order, AssetPairSettings assetPairSettings);
    }
}

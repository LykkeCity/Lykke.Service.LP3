using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Settings;

namespace Lykke.Service.LP3.Domain.Services
{
    public interface IOrdersConverter
    {
        LimitOrder Convert(LimitOrder order, AssetPairSettings assetPairSettings);
    }
}

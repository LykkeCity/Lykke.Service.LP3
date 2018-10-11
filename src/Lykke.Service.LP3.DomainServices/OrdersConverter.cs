using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Services;
using Lykke.Service.LP3.Domain.Settings;

namespace Lykke.Service.LP3.DomainServices
{
    public class OrdersConverter : IOrdersConverter
    {
        public LimitOrder Convert(LimitOrder order, AssetPairSettings assetPairSettings)
        {
            throw new System.NotImplementedException();
        }
    }
}

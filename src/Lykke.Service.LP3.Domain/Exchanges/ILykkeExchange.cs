using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.Assets.Client.Models.v3;
using Lykke.Service.LP3.Domain.Orders;

namespace Lykke.Service.LP3.Domain.Exchanges
{
    public interface ILykkeExchange
    {
        Task ApplyAsync(AssetPair assetPair, IReadOnlyList<LimitOrder> limitOrders);
    }
}

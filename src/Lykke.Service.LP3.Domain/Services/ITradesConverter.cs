using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Settings;

namespace Lykke.Service.LP3.Domain.Services
{
    public interface ITradesConverter
    {
        Task<IEnumerable<Trade>> ConvertAsync(IReadOnlyList<Trade> trades, AssetPairSettings assetPairSettings);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Domain.Settings;
using Lykke.Service.LP3.Domain.TradingAlgorithm;

namespace Lykke.Service.LP3.Domain.Services
{
    public interface IOrderBookTraderService
    {
        Task<IReadOnlyCollection<OrderBookTrader>> GetOrderBookTradersAsync();

        Task AddOrderBookTraderAsync(OrderBookTraderSettings orderBookTraderSettings);

        Task UpdateOrderBookTraderSettingsAsync(OrderBookTraderSettings orderBookTraderSettings);

        Task DeleteOrderBookAsync(string assetPairId);

        Task UpdateOrderBookTraderAsync(OrderBookTrader orderBookTrader);
    }
}

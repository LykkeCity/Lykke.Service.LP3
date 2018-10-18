using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Domain.TradingAlgorithm;

namespace Lykke.Service.LP3.Domain.Repositories
{
    public interface IOrderBookTraderRepository
    {
        Task<IReadOnlyList<OrderBookTrader>> GetAllAsync();
        Task AddAsync(OrderBookTrader orderBookSettings);
        Task UpdateAsync(OrderBookTrader orderBookTrader);
        Task DeleteAsync(string assetPairId);
    }
}

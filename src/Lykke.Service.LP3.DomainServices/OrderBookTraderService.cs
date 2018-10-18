using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.LP3.Domain.Repositories;
using Lykke.Service.LP3.Domain.Services;
using Lykke.Service.LP3.Domain.Settings;
using Lykke.Service.LP3.Domain.TradingAlgorithm;

namespace Lykke.Service.LP3.DomainServices
{
    public class OrderBookTraderService : IOrderBookTraderService
    {
        private readonly IOrderBookTraderRepository _orderBookTraderRepository;
        private readonly ILog _log;

        private Dictionary<string, OrderBookTrader> _orderBookTraders;

        public OrderBookTraderService(ILogFactory logFactory,
            IOrderBookTraderRepository orderBookTraderRepository)
        {
            _orderBookTraderRepository = orderBookTraderRepository;
            _log = logFactory.CreateLog(this);
        }
        
        public async Task<IReadOnlyCollection<OrderBookTrader>> GetOrderBookTradersAsync()
        {
            if (_orderBookTraders == null)
            {
                _orderBookTraders =
                    (await _orderBookTraderRepository.GetAllAsync()).ToDictionary(x => x.AssetPairId, x => x);
            }

            return _orderBookTraders.Values;
        }

        public async Task AddOrderBookTraderAsync(OrderBookTraderSettings orderBookSettings)
        {
            var orderBook = new OrderBookTrader(orderBookSettings);
            
            await _orderBookTraderRepository.AddAsync(orderBook);
            
            _orderBookTraders.Add(orderBook.AssetPairId, orderBook);
        }

        public Task UpdateOrderBookTraderSettingsAsync(OrderBookTraderSettings orderBookSettings)
        {
            _orderBookTraders[orderBookSettings.AssetPairId].UpdateSettings(orderBookSettings);

            return UpdateOrderBookTraderAsync(_orderBookTraders[orderBookSettings.AssetPairId]);
        }

        public async Task DeleteOrderBookAsync(string assetPairId)
        {
            await _orderBookTraderRepository.DeleteAsync(assetPairId);

            _orderBookTraders.Remove(assetPairId);
        }

        public Task UpdateOrderBookTraderAsync(OrderBookTrader orderBookTrader)
        {
            return _orderBookTraderRepository.UpdateAsync(orderBookTrader);
        }
    }
}

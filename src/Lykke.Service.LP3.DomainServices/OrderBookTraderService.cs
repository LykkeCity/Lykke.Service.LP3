using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
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
                _orderBookTraders = (await _orderBookTraderRepository.GetAllAsync())
                    .ToDictionary(x => x.AssetPairId, x => x, StringComparer.InvariantCultureIgnoreCase);
                
                _log.Info("OrderBookTraders cache is initialized", 
                    context: $"traders: [{string.Join(", ", _orderBookTraders.Values.Select(x => x.ToJson()))}]");
            }

            return _orderBookTraders.Values;
        }

        public async Task AddOrderBookTraderAsync([NotNull] OrderBookTraderSettings orderBookSettings)
        {
            if (orderBookSettings == null) throw new ArgumentNullException(nameof(orderBookSettings));
            
            var orderBook = new OrderBookTrader(orderBookSettings);
            
            await _orderBookTraderRepository.AddAsync(orderBook);
            
            _orderBookTraders.Add(orderBook.AssetPairId, orderBook);
        }

        public async Task UpdateOrderBookTraderSettingsAsync([NotNull] OrderBookTraderSettings orderBookSettings)
        {
            if (orderBookSettings == null) throw new ArgumentNullException(nameof(orderBookSettings));
            
            var trader = GetTraderByAssetPairId(orderBookSettings.AssetPairId);

            string traderStateBefore = trader.ToJson();
            
            if (trader == null)
            {
                _log.Warning("Unable to update settings of non-existing OrderBookTrader", context: orderBookSettings.AssetPairId);
                return;
            }
            
            _orderBookTraders[orderBookSettings.AssetPairId].UpdateSettings(orderBookSettings);

            await PersistOrderBookTraderAsync(_orderBookTraders[orderBookSettings.AssetPairId]);
            
            _log.Info("Updating OrderBookTrader settings",
                context: $"before: {traderStateBefore}, after: {_orderBookTraders[orderBookSettings.AssetPairId].ToJson()}");
        }

        public async Task DeleteOrderBookAsync(string assetPairId)
        {
            var trader = GetTraderByAssetPairId(assetPairId);
            if (trader == null)
            {
                _log.Warning("Unable to delete non-existing OrderBookTrader", context: assetPairId);
                return;
            }
            
            await _orderBookTraderRepository.DeleteAsync(assetPairId);

            _orderBookTraders.Remove(assetPairId);
            
            _log.Info("OrderBookTrader was removed", context: $"trader: {trader.ToJson()}");
        }

        public Task PersistOrderBookTraderAsync([NotNull] OrderBookTrader orderBookTrader)
        {
            if (orderBookTrader == null) throw new ArgumentNullException(nameof(orderBookTrader));
            
            return _orderBookTraderRepository.UpdateAsync(orderBookTrader);
        }

        public OrderBookTrader GetTraderByAssetPairId(string assetPairId)
        {
            return _orderBookTraders[assetPairId];
        }
    }
}

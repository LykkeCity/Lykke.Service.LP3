using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Services;
using Lykke.Service.LP3.Domain.Settings;

namespace Lykke.Service.LP3.DomainServices
{
    public class AdditionalVolumeService : IAdditionalVolumeService
    {
        private readonly ISettingsService _settingsService;
        private readonly ILog _log;

        public AdditionalVolumeService(ILogFactory logFactory,
            ISettingsService settingsService)
        {
            _settingsService = settingsService;
            _log = logFactory.CreateLog(this);
        }
        
        public async Task<IEnumerable<LimitOrder>> GetOrdersAsync(IEnumerable<LimitOrder> currentOrders)
        {
            if (!TryGetBaseBidAsk(currentOrders.ToList(), out var bid, out var ask))
            {
                return Enumerable.Empty<LimitOrder>();
            }

            var settings = await _settingsService.GetAdditionalVolumeSettingsAsync();
            if (settings == null)
            {
                return Enumerable.Empty<LimitOrder>();
            }

            var asks = GetOrders(ask, settings, TradeType.Sell);
            var bids = GetOrders(bid, settings, TradeType.Buy);

            return asks.Union(bids);
        }

        private bool TryGetBaseBidAsk(ICollection<LimitOrder> currentOrders, out decimal bid, out decimal ask)
        {
            var asks = currentOrders.Where(x => x.TradeType == TradeType.Sell).OrderBy(x => x.Price).ToList();
            var bids = currentOrders.Where(x => x.TradeType == TradeType.Buy).OrderBy(x => x.Price).ToList();

            var bestBid = bids.LastOrDefault();
            var bestAsk = asks.FirstOrDefault();
            
            var worstBid = bids.FirstOrDefault();
            var worstAsk = asks.LastOrDefault();

            if (worstAsk == null && worstBid == null)
            {
                _log.Info("No current orders to create additional orders");
                ask = bid = 0;
                return false;
            }

            bid = worstBid?.Price ?? bestAsk.Price;
            ask = worstAsk?.Price ?? bestBid.Price;

            return true;
        }

        private IEnumerable<LimitOrder> GetOrders(decimal worstPrice, AdditionalVolumeSettings settings, TradeType tradeType)
        {
            decimal price = worstPrice;
            
            for (int i = 0; i < settings.Count; i++)
            {
                price = tradeType == TradeType.Sell
                    ? (decimal) Math.Exp(Math.Log((double) price) + (double) settings.Delta)
                    : (decimal) Math.Exp(Math.Log((double) price) - (double) settings.Delta);
                
                yield return new LimitOrder(price, settings.Volume, tradeType);
            }
        }
    }
}

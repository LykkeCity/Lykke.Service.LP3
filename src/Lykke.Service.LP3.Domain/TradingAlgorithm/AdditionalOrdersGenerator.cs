using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.Service.LP3.Domain.Orders;

namespace Lykke.Service.LP3.Domain.TradingAlgorithm
{
    public class AdditionalOrdersGenerator
    {
        public IEnumerable<LimitOrder> GetOrders(IEnumerable<LimitOrder> currentOrders,
            int count, decimal volume, decimal delta)
        {
            if (!TryGetBaseBidAsk(currentOrders.ToList(), out var bid, out var ask))
            {
                return Enumerable.Empty<LimitOrder>();
            }
            
            var asks = GetOrders(ask, count, volume, delta, TradeType.Sell);
            var bids = GetOrders(bid, count, volume, delta, TradeType.Buy);

            return asks.Union(bids);
        }

        private bool TryGetBaseBidAsk(ICollection<LimitOrder> currentOrders, out decimal bid, out decimal ask)
        {
            var asks = currentOrders.Where(x => x.TradeType == TradeType.Sell).OrderBy(x => x.Price).ToList();
            var bids = currentOrders.Where(x => x.TradeType == TradeType.Buy).OrderBy(x => x.Price).ToList();

            if (!asks.Any() && !bids.Any())
            {
                ask = bid = 0;
                return false;
            }
            
            var bestBid = bids.LastOrDefault();
            var bestAsk = asks.FirstOrDefault();
            
            var worstBid = bids.FirstOrDefault();
            var worstAsk = asks.LastOrDefault();

            bid = worstBid?.Price ?? bestAsk.Price;
            ask = worstAsk?.Price ?? bestBid.Price;

            return true;
        }

        private IEnumerable<LimitOrder> GetOrders(decimal worstPrice, int count, decimal volume, decimal delta, 
            TradeType tradeType)
        {
            decimal price = worstPrice;
            
            for (int i = 0; i < count; i++)
            {
                price = tradeType == TradeType.Sell
                    ? (decimal) Math.Exp(Math.Log((double) price) + (double) delta)
                    : (decimal) Math.Exp(Math.Log((double) price) - (double) delta);
                
                var order = new LimitOrder(price, volume, tradeType);

                yield return order;
            }
        }
    }
}

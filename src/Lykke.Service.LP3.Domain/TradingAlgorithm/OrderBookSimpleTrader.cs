using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Repositories;

namespace Lykke.Service.LP3.Domain.TradingAlgorithm
{
    public class OrderBookSimpleTrader : IStartable
    {
        private readonly ILimitOrderRepository _limitOrderRepository;
        public string AssetPairId { get; }
        
        public int Count { get; private set; }
        
        public decimal Delta { get; private set; }
        
        public decimal Volume { get; private set; }
        
        public decimal InitialPrice { get; private set; }

        private readonly LinkedList<LimitOrder> _orders = new LinkedList<LimitOrder>();

        public OrderBookSimpleTrader(string assetPairId, int count, decimal delta, decimal volume, decimal initialPrice,
            ILimitOrderRepository limitOrderRepository)
        {
            _limitOrderRepository = limitOrderRepository;
            AssetPairId = assetPairId;
            Count = count;
            Delta = delta;
            Volume = volume;
            InitialPrice = initialPrice;
        }

        public async Task<IReadOnlyCollection<LimitOrder>> GenerateOrdersAsync()
        {
            _orders.Clear();
            await _limitOrderRepository.ClearAsync();
            
            
            decimal price = InitialPrice;

            for (int i = 0; i < Count; i++)
            {
                price = AddDelta(price);
                
                _orders.AddLast(new LimitOrder(price, Volume, TradeType.Sell)
                {
                    Number = i + 1
                });
            }

            price = InitialPrice;

            for (int i = 0; i < Count; i++)
            {
                price = SubtractDelta(price);
                
                _orders.AddLast(new LimitOrder(price, Volume, TradeType.Buy)
                {
                    Number = i + 1
                });
            }

            await _limitOrderRepository.AddBatchAsync(_orders);
            return _orders;
        }
        
        private decimal AddDelta(decimal price) => (decimal) Math.Exp(Math.Log((double) price) + (double) Delta);
        
        private decimal SubtractDelta(decimal price) => (decimal) Math.Exp(Math.Log((double) price) - (double) Delta);

        public void HandleTrade(Trade trade)
        {
            if (trade.Type == TradeType.None) throw new ArgumentException("Trade has None type", nameof(trade));

            SpreadVolumeOnOrders(
                trade.Type == TradeType.Sell
                    ? _orders.Where(x => x.TradeType == TradeType.Sell).OrderBy(x => x.Price)
                    : _orders.Where(x => x.TradeType == TradeType.Buy).OrderByDescending(x => x.Price), 
                trade.Volume);
        }

        private void SpreadVolumeOnOrders(IOrderedEnumerable<LimitOrder> orders, decimal volume)
        {
            foreach (var limitOrder in orders)
            {
                if (limitOrder.Volume <= volume)
                {
                    _orders.Remove(limitOrder);
                    _orders.AddLast(CreateOppositeOrder(limitOrder));
                    
                    volume -= limitOrder.Volume;
                }
                else
                {
                    limitOrder.Volume -= volume;
                }

                if (volume == 0)
                {
                    break;
                }
            }
        }

        private LimitOrder CreateOppositeOrder(LimitOrder executedOrder)
        {
            return executedOrder.TradeType == TradeType.Sell ? 
                new LimitOrder(SubtractDelta(executedOrder.Price), Volume, TradeType.Buy) : 
                new LimitOrder(AddDelta(executedOrder.Price), Volume, TradeType.Sell);
        }

        public void Start()
        {
            throw new NotImplementedException();
        }
    }
}

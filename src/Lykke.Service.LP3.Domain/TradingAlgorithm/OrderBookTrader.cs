using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Settings;
using MoreLinq;
using LimitOrder = Lykke.Service.LP3.Domain.Orders.LimitOrder;

namespace Lykke.Service.LP3.Domain.TradingAlgorithm
{
    public class OrderBookTrader
    {
        public string AssetPairId { get; }

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                MarkOrdersIfDisabled(_orders);
            }
        }

        public decimal Delta { get; private set; }
        public decimal Volume { get; private set; }
        public int Count { get; private set; }
        public decimal InitialPrice { get; private set; }

        public decimal Inventory { get; private set; }
        public decimal OppositeInventory { get; private set; }

        private readonly LinkedList<LimitOrder> _orders = new LinkedList<LimitOrder>();
        private bool _isEnabled;

        public OrderBookTrader(OrderBookTraderSettings settings)
        {
            AssetPairId = settings.AssetPairId;
            IsEnabled = settings.IsEnabled;
            InitialPrice = settings.InitialPrice;
            
            Delta = settings.Delta;
            Volume = settings.Volume;
            Count = settings.Count;
        }
        
        [UsedImplicitly] // used by Mapper
        public OrderBookTrader(string assetPairId, bool isEnabled, decimal initialPrice, decimal delta, 
            decimal volume, int count, decimal inventory, decimal oppositeInventory) 
            : this(new OrderBookTraderSettings
                {
                    AssetPairId = assetPairId,
                    IsEnabled = isEnabled,
                    Delta = delta,
                    Volume = volume,
                    Count = count,
                    InitialPrice = initialPrice
                })
        {
            Inventory = inventory;
            OppositeInventory = oppositeInventory;
        }
        
        public IReadOnlyCollection<LimitOrder> CreateOrders()
        {
            _orders.Clear();
            
            CreateOrders(InitialPrice, TradeType.Sell).ForEach(x => _orders.AddLast(x));
            CreateOrders(InitialPrice, TradeType.Buy).ForEach(x => _orders.AddLast(x));
            
            MarkOrdersIfDisabled(_orders);

            return _orders;
        }
        
        private IEnumerable<LimitOrder> CreateOrders(decimal initialPrice, TradeType tradeType)
        {
            decimal price = initialPrice;
            
            for (int i = 0; i < Count; i++)
            {
                price = tradeType == TradeType.Sell ? AddDelta(price) : SubtractDelta(price);

                yield return new LimitOrder(price, Volume, tradeType)
                    {
                        AssetPairId = AssetPairId,
                        Number = i + 1
                    };
            }
        }
        
        private decimal AddDelta(decimal price) => (decimal) Math.Exp(Math.Log((double) price) + (double) Delta);
        
        private decimal SubtractDelta(decimal price) => (decimal) Math.Exp(Math.Log((double) price) - (double) Delta);

        private void MarkOrdersIfDisabled(IEnumerable<LimitOrder> orders)
        {
            if (!IsEnabled)
            {
                orders.ForEach(x =>
                {
                    x.Error = LimitOrderError.OrderBookIsDisabled;
                    x.ErrorMessage = "Order book is disabled";
                });
            }
            else
            {
                orders.ForEach(x =>
                {
                    x.Error = LimitOrderError.None;
                    x.ErrorMessage = null;
                });
            }
        }

        public IEnumerable<LimitOrder> HandleTrades(IReadOnlyCollection<Trade> trades)
        {
            return trades.Select(HandleTrade).SelectMany(x => x);
        }

        private IEnumerable<LimitOrder> HandleTrade(Trade trade)
        {
            if (trade.Type == TradeType.None) throw new ArgumentException("Trade has None type", nameof(trade));

            return SpreadVolumeOnOrders(
                trade.Type == TradeType.Sell
                    ? _orders.Where(x => x.TradeType == TradeType.Sell).OrderBy(x => x.Price)
                    : _orders.Where(x => x.TradeType == TradeType.Buy).OrderByDescending(x => x.Price), 
                trade.Volume);
        }
        
        private IEnumerable<LimitOrder> SpreadVolumeOnOrders(IOrderedEnumerable<LimitOrder> orders, decimal volume)
        {
            foreach (var limitOrder in orders)
            {
                LimitOrder newOrder = null;
                
                if (limitOrder.Volume <= volume)
                {
                    _orders.Remove(limitOrder);

                    newOrder = CreateOppositeOrder(limitOrder);
                    _orders.AddLast(newOrder);
                    
                    volume -= limitOrder.Volume;

                    if (limitOrder.TradeType == TradeType.Sell)
                    {
                        Inventory -= limitOrder.Volume;
                        OppositeInventory += limitOrder.Volume * limitOrder.Price;
                    }
                    else
                    {
                        Inventory += limitOrder.Volume;
                        OppositeInventory -= limitOrder.Volume * limitOrder.Price;
                    }
                }
                else
                {
                    limitOrder.Volume -= volume;
                    
                    if (limitOrder.TradeType == TradeType.Sell)
                    {
                        Inventory -= volume;
                        OppositeInventory += volume * limitOrder.Price;
                    }
                    else
                    {
                        Inventory += volume;
                        OppositeInventory -= volume * limitOrder.Price;
                    }
                }
                
                if (volume == 0)
                {
                    break;
                }

                if (newOrder != null)
                {
                    yield return newOrder;
                }
            }
        }
        
        private LimitOrder CreateOppositeOrder(LimitOrder executedOrder)
        {
            return executedOrder.TradeType == TradeType.Sell
                ? new LimitOrder(SubtractDelta(executedOrder.Price), Volume, TradeType.Buy)
                    {
                        Number = executedOrder.Number - 1
                    }
                : new LimitOrder(AddDelta(executedOrder.Price), Volume, TradeType.Sell)
                    {
                        Number = executedOrder.Number + 1
                    };
        }

        public void UpdateSettings(OrderBookTraderSettings settings)
        {
            IsEnabled = settings.IsEnabled;
            MarkOrdersIfDisabled(_orders);

            if (settings.InitialPrice != 0)
            {
                InitialPrice = settings.InitialPrice;
            }
            
            Delta = settings.Delta;
            Count = settings.Count;
            Volume = settings.Volume;    
        }

        public IReadOnlyCollection<LimitOrder> GetOrders()
        {
            return _orders;
        }

        public void AddOrderManually([NotNull] LimitOrder limitOrder)
        {
            if (limitOrder == null) throw new ArgumentNullException(nameof(limitOrder));

            if (!string.Equals(limitOrder.AssetPairId, AssetPairId, StringComparison.InvariantCultureIgnoreCase))
                throw new ArgumentException("LimitOrder is for another AssetPair");
            
            _orders.AddLast(limitOrder);
        }

        public LimitOrder CancelOrder(Guid orderId)
        {
            var order = _orders.SingleOrDefault(x => x.Id == orderId);
            _orders.Remove(order);
            return order;
        }

        public void Clear()
        {
            _orders.Clear();
        }

        public void RestoreOrders(IEnumerable<LimitOrder> limitOrders)
        {
            _orders.Clear();
            
            limitOrders.ForEach(x => _orders.AddLast(x));
        }
    }
}

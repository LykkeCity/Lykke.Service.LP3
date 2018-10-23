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
        public bool IsEnabled { get; private set; }
        public decimal Delta { get; private set; }
        public decimal Volume { get; private set; }
        public int Count { get; private set; }
        public decimal InitialPrice { get; private set; }

        public decimal Inventory { get; private set; }
        public decimal OppositeInventory { get; private set; }

        private readonly LinkedList<LimitOrder> _orders = new LinkedList<LimitOrder>();

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
        }

        public void HandleTrades(IReadOnlyCollection<Trade> trades)
        {
            trades.ForEach(HandleTrade);
        }

        private void HandleTrade(Trade trade)
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
            }
        }
        
        private LimitOrder CreateOppositeOrder(LimitOrder executedOrder)
        {
            return executedOrder.TradeType == TradeType.Sell ? 
                new LimitOrder(SubtractDelta(executedOrder.Price), Volume, TradeType.Buy) : 
                new LimitOrder(AddDelta(executedOrder.Price), Volume, TradeType.Sell);
        }

        public void UpdateSettings(OrderBookTraderSettings settings)
        {
            IsEnabled = settings.IsEnabled;

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
                throw new ArgumentException($"LimitOrder is for another AssetPair");
            
            _orders.AddLast(limitOrder);
        }
    }
}

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
        
        public int CountInMarket { get; private set; }
        public bool IsReverseBook { get; private set; }
        public int VolumeAccuracy { get; private set; }

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
            CountInMarket = settings.CountInMarket;
            IsReverseBook = settings.IsReverseBook;
            VolumeAccuracy = settings.VolumeAccuracy;
        }
        
        [UsedImplicitly] // used by Mapper
        public OrderBookTrader(string assetPairId, bool isEnabled, decimal initialPrice, decimal delta, 
            decimal volume, int count, int countInMarket, bool IsReverseBook, int volumeAccuracy, decimal inventory, decimal oppositeInventory) 
            : this(new OrderBookTraderSettings
                {
                    AssetPairId = assetPairId,
                    IsEnabled = isEnabled,
                    Delta = delta,
                    Volume = volume,
                    Count = count,
                    InitialPrice = initialPrice,
                    CountInMarket = countInMarket,
                    IsReverseBook = IsReverseBook,
                    VolumeAccuracy = volumeAccuracy
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

        public (IReadOnlyCollection<LimitOrder> addedOrders, IReadOnlyCollection<LimitOrder> removedOrders) 
            HandleTrades(IReadOnlyCollection<Trade> trades, decimal minVolume)
        {
            var addedOrders = new List<LimitOrder>();
            var removedOrders = new List<LimitOrder>();
            
            foreach (var trade in trades)
            {
                var (addedOrdersFromTrade, removedOrdersFromTrade) = HandleTrade(trade, minVolume);
                addedOrders.AddRange(addedOrdersFromTrade);
                removedOrders.AddRange(removedOrdersFromTrade);
            }

            return (addedOrders, removedOrders);
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
            CountInMarket = settings.CountInMarket;
            IsReverseBook = settings.IsReverseBook;
        }

        public IReadOnlyCollection<LimitOrder> GetOrders()
        {
            return _orders;
        }

        public LimitOrder AddOrderManually([NotNull] LimitOrder limitOrder)
        {
            if (limitOrder == null) throw new ArgumentNullException(nameof(limitOrder));

            if (!string.Equals(limitOrder.AssetPairId, AssetPairId, StringComparison.InvariantCultureIgnoreCase))
                throw new ArgumentException("LimitOrder is for another AssetPair");
            
            _orders.AddLast(limitOrder);
            
            MarkOrdersIfDisabled(_orders);

            return limitOrder;
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

        private (IReadOnlyCollection<LimitOrder> addedOrders, IReadOnlyCollection<LimitOrder> removedOrders) 
            HandleTrade(Trade trade, decimal minVolume)
        {
            if (trade.Type == TradeType.None) throw new ArgumentException("Trade has None type", nameof(trade));

            return SpreadVolumeOnOrders(
                trade.Type == TradeType.Sell
                    ? _orders.Where(x => x.TradeType == TradeType.Sell).OrderBy(x => x.Price)
                    : _orders.Where(x => x.TradeType == TradeType.Buy).OrderByDescending(x => x.Price), 
                trade.Volume, trade.Price, minVolume);
        }
        
        private (IReadOnlyCollection<LimitOrder> addedOrders, IReadOnlyCollection<LimitOrder> removedOrders) 
            SpreadVolumeOnOrders(IOrderedEnumerable<LimitOrder> orders, decimal volume, decimal tradePrice, decimal minVolume)
        {
            var addedOrders = new List<LimitOrder>();
            var removedOrders = new List<LimitOrder>();
            
            foreach (var limitOrder in orders)
            {
                if (limitOrder.Volume <= volume || limitOrder.Volume - volume < minVolume)
                {
                    _orders.Remove(limitOrder);
                    removedOrders.Add(limitOrder);
                    
                    var newOrder = CreateOppositeOrder(limitOrder);
                    _orders.AddLast(newOrder);
                    addedOrders.Add(newOrder);
                    
                    if (limitOrder.TradeType == TradeType.Sell)
                    {
                        Inventory -= limitOrder.Volume;
                        OppositeInventory += limitOrder.Volume * tradePrice;
                    }
                    else
                    {
                        Inventory += limitOrder.Volume;
                        OppositeInventory -= limitOrder.Volume * tradePrice;
                    }

                    volume -= limitOrder.Volume;
                    
                    if (volume <= 0)
                    {
                        break;
                    }
                }
                else
                {
                    limitOrder.Volume -= volume;
                    
                    if (limitOrder.TradeType == TradeType.Sell)
                    {
                        Inventory -= volume;
                        OppositeInventory += volume * tradePrice;
                    }
                    else
                    {
                        Inventory += volume;
                        OppositeInventory -= volume * tradePrice;
                    }

                    break;
                }
            }

            return (addedOrders, removedOrders);
        }
        
        //private decimal AddDelta(decimal price) => (decimal) Math.Exp(Math.Log((double) price) + (double) Delta);
        //private decimal SubtractDelta(decimal price) => (decimal) Math.Exp(Math.Log((double) price) - (double) Delta);

        private decimal AddDelta(decimal price) => price + Delta;
        private decimal SubtractDelta(decimal price) => price - Delta;

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
                orders.Where(x => x.Error == LimitOrderError.OrderBookIsDisabled).ForEach(x =>
                {
                    x.Error = LimitOrderError.None;
                    x.ErrorMessage = null;
                });
            }
        }

        private IEnumerable<LimitOrder> CreateOrders(decimal initialPrice, TradeType tradeType)
        {
            var orderList = new List<LimitOrder>();

            decimal price = initialPrice;

            for (int i = 0; i < Count; i++)
            {
                price = tradeType == TradeType.Sell ? AddDelta(price) : SubtractDelta(price);

                if (price > 0)
                {
                    decimal number = tradeType == TradeType.Sell ? i + 1 : -(i + 1);

                    var volume = CalculateVolume(price);

                    var order = new LimitOrder(price, volume, tradeType, AssetPairId, number);
                    orderList.Add(order);
                }
            }

            return orderList;
        }

        private LimitOrder CreateOppositeOrder(LimitOrder executedOrder)
        {
            LimitOrder order;

            if (executedOrder.TradeType == TradeType.Sell)
            {
                var price = SubtractDelta(executedOrder.Price);
                var volume = CalculateVolume(price);
                order = new LimitOrder(price, volume, TradeType.Buy, AssetPairId, executedOrder.Number - 1);
            }
            else
            {
                var price = AddDelta(executedOrder.Price);
                var volume = CalculateVolume(price);
                order = new LimitOrder(price, volume, TradeType.Sell, AssetPairId, executedOrder.Number + 1);
            }

            return order;
        }

        private decimal CalculateVolume(decimal price)
        {
            var volume = IsReverseBook ? Math.Round(Volume / price, VolumeAccuracy) : Volume;
            return volume;
        }
    }
}

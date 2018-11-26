using System;
using System.Collections.Generic;
using System.Linq;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
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
        
        public int MinCountOrderInMarket { get; private set; }
        public int AddedCountOrdersInMarket { get; private set; }

        private int MaxCountOrderInMarket => MinCountOrderInMarket + AddedCountOrdersInMarket;

        private readonly LinkedList<LimitOrder> _orders = new LinkedList<LimitOrder>();
        private bool _isEnabled;

        private LimitOrder CheapestSellOrder =>
            _orders.Where(x => x.TradeType == TradeType.Sell).OrderBy(x => x.Price).First();
        private LimitOrder MostExpensiceSellOrder =>
            _orders.Where(x => x.TradeType == TradeType.Sell).OrderByDescending(x => x.Price).First();
        private LimitOrder CheapestBuyOrder =>
            _orders.Where(x => x.TradeType == TradeType.Buy).OrderBy(x => x.Price).First();
        private LimitOrder MostExpensiceBuyOrder =>
            _orders.Where(x => x.TradeType == TradeType.Buy).OrderByDescending(x => x.Price).First();

        private readonly ILog _log;

        public OrderBookTrader(OrderBookTraderSettings settings, ILogFactory logFactory)
        {
            if(settings.MinCountOrderInMarket + settings.AddedCountOrdersInMarket > settings.Count)
                throw new ArgumentException($"Inconsistent settings for trader: {nameof(settings.Count)} ({settings.Count}) should be more than {nameof(settings.MinCountOrderInMarket)} ({settings.MinCountOrderInMarket}) + {nameof(settings.AddedCountOrdersInMarket)} ({settings.AddedCountOrdersInMarket})");
            
            AssetPairId = settings.AssetPairId;
            IsEnabled = settings.IsEnabled;
            InitialPrice = settings.InitialPrice;
            
            Delta = settings.Delta;
            Volume = settings.Volume;
            Count = settings.Count;

            MinCountOrderInMarket = settings.MinCountOrderInMarket;
            AddedCountOrdersInMarket = settings.AddedCountOrdersInMarket;

            _log = logFactory.CreateLog(this);
        }
        
        [UsedImplicitly] // used by Mapper
        public OrderBookTrader(string assetPairId, bool isEnabled, decimal initialPrice, decimal delta, 
            decimal volume, int count, decimal inventory, decimal oppositeInventory, ILogFactory logFactory) 
            : this(new OrderBookTraderSettings
                {
                    AssetPairId = assetPairId,
                    IsEnabled = isEnabled,
                    Delta = delta,
                    Volume = volume,
                    Count = count,
                    InitialPrice = initialPrice
                },
                logFactory)
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

        public (IReadOnlyCollection<LimitOrder> addedOrders, IReadOnlyCollection<LimitOrder> removedOrders, IReadOnlyCollection<LimitOrder> toCancelOrders) 
            HandleTrades(IReadOnlyCollection<Trade> trades, decimal minVolume)
        {
            var addedOrders = new List<LimitOrder>();
            var removedOrders = new List<LimitOrder>();
            var toCancelOrders = new List<LimitOrder>();
            
            foreach (var trade in trades)
            {
                var (addedOrdersFromTrade, removedOrdersFromTrade, toCancelOrdersFromTrade) = HandleTrade(trade, minVolume);
                addedOrders.AddRange(addedOrdersFromTrade);
                removedOrders.AddRange(removedOrdersFromTrade);
                toCancelOrders.AddRange(toCancelOrdersFromTrade);
            }

            return (addedOrders, removedOrders, toCancelOrders);
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
            MinCountOrderInMarket = settings.MinCountOrderInMarket;
            AddedCountOrdersInMarket = settings.AddedCountOrdersInMarket;
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
            
            MarkOrdersIfDisabled(_orders);
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

        private (IReadOnlyCollection<LimitOrder> addedOrders, IReadOnlyCollection<LimitOrder> removedOrders, IReadOnlyCollection<LimitOrder> toCancelOrders) 
            HandleTrade(Trade trade, decimal minVolume)
        {
            if (trade.Type == TradeType.None) throw new ArgumentException("Trade has None type", nameof(trade));

            return SpreadVolumeOnOrders(
                trade.Type == TradeType.Sell
                    ? _orders.Where(x => x.TradeType == TradeType.Sell).OrderBy(x => x.Price)
                    : _orders.Where(x => x.TradeType == TradeType.Buy).OrderByDescending(x => x.Price), 
                trade.Type, trade.Volume, minVolume);
        }
        
        private (IReadOnlyCollection<LimitOrder> addedOrders, IReadOnlyCollection<LimitOrder> removedOrders, IReadOnlyCollection<LimitOrder> toCancelOrders) 
            SpreadVolumeOnOrders(IOrderedEnumerable<LimitOrder> orders, TradeType tradeType, decimal volume, decimal minVolume)
        {
            var addedOrders = new List<LimitOrder>();
            var removedOrders = new List<LimitOrder>();
            var toCancelOrders = new List<LimitOrder>();
            
            foreach (var limitOrder in orders)
            {
                if (limitOrder.Volume <= volume || limitOrder.Volume - volume < minVolume)
                {
                    _orders.Remove(limitOrder);
                    removedOrders.Add(limitOrder);
                    
                    
                    var threshold = InitialPrice +
                                    Delta * Count * (tradeType == TradeType.Sell ? 1m : -1m);

                    var newOrder = CreateOppositeOrder(limitOrder);

                    if (tradeType == TradeType.Buy && newOrder.Price >= threshold ||
                        tradeType == TradeType.Sell && newOrder.Price <= threshold)
                    {
                        _orders.AddLast(newOrder);
                        addedOrders.Add(newOrder);
                    }

                    if (_orders.Count(x => x.TradeType == tradeType) <= MinCountOrderInMarket)
                    {
                        var numOfOrdersToAdd =
                            Math.Min(
                                Convert.ToInt32(
                                    (tradeType == TradeType.Sell
                                        ? threshold - MostExpensiceSellOrder.Price
                                        : threshold - CheapestBuyOrder.Price)
                                    / Delta),
                                AddedCountOrdersInMarket);
                        
                        CreateOrders(
                                tradeType == TradeType.Buy
                                ? CheapestBuyOrder.Price
                                : MostExpensiceSellOrder.Price,
                            tradeType,
                            numOfOrdersToAdd)
                            .ForEach(
                                x =>
                                {
                                    _orders.AddLast(x);
                                    addedOrders.Add(x);
                                });

                        for (int i = 0; i < numOfOrdersToAdd; i++)
                        {
                            var orderToRemove = tradeType == TradeType.Sell ? CheapestBuyOrder : MostExpensiceBuyOrder;

                            _orders.Remove(orderToRemove);
                            toCancelOrders.Add(orderToRemove);
                        }
                    }
                    
                    if (tradeType == TradeType.Sell)
                    {
                        Inventory -= limitOrder.Volume;
                        OppositeInventory += limitOrder.Volume * limitOrder.Price;
                    }
                    else
                    {
                        Inventory += limitOrder.Volume;
                        OppositeInventory -= limitOrder.Volume * limitOrder.Price;
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
                        OppositeInventory += volume * limitOrder.Price;
                    }
                    else
                    {
                        Inventory += volume;
                        OppositeInventory -= volume * limitOrder.Price;
                    }

                    break;
                }
            }

            if (volume > minVolume)
            {
                _log.WriteWarning(nameof(SpreadVolumeOnOrders), AssetPairId, $"Volume {volume} left for {tradeType.ToString()}");
            }

            return (addedOrders, removedOrders, toCancelOrders);
        }
        
        private IEnumerable<LimitOrder> CreateOrders(decimal initialPrice, TradeType tradeType, int? numOfOrders = default(int?))
        {
            decimal price = initialPrice;
            
            for (int i = 0; i < (numOfOrders ?? MaxCountOrderInMarket); i++)
            {
                price = tradeType == TradeType.Sell ? AddDelta(price) : SubtractDelta(price);

                decimal number = tradeType == TradeType.Sell ? i + 1 : -(i + 1);

                yield return new LimitOrder(price, Volume, tradeType, AssetPairId, number);
            }
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
        
        private LimitOrder CreateOppositeOrder(LimitOrder executedOrder)
        {
            return executedOrder.TradeType == TradeType.Sell
                ? new LimitOrder(SubtractDelta(executedOrder.Price), Volume, TradeType.Buy, AssetPairId, executedOrder.Number - 1)
                : new LimitOrder(AddDelta(executedOrder.Price), Volume, TradeType.Sell, AssetPairId, executedOrder.Number + 1);
        }
    }
}

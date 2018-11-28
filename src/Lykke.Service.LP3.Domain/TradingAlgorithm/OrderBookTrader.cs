using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.LP3.Domain.Exchanges;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Services;
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
                _limitOrderStore.MarkOrdersDisabled(!_isEnabled).GetAwaiter().GetResult();
            }
        }

        private bool _isEnabled;

        public decimal Delta { get; private set; }
        public decimal Volume { get; private set; }
        public int Count { get; private set; }
        public decimal InitialPrice { get; private set; }

        public decimal Inventory { get; private set; }
        public decimal OppositeInventory { get; private set; }

        private readonly ILimitOrderStore _limitOrderStore;
        private readonly ILykkeExchange _lykkeExchange;

        public OrderBookTrader(OrderBookTraderSettings settings, ILimitOrderStore limitOrderStore, ILykkeExchange lykkeExchange)
        {
            AssetPairId = settings.AssetPairId;
            _isEnabled = settings.IsEnabled;
            InitialPrice = settings.InitialPrice;
            
            Delta = settings.Delta;
            Volume = settings.Volume;
            Count = settings.Count;

            _limitOrderStore = limitOrderStore;
            _lykkeExchange = lykkeExchange;
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
                }, null, null)
        {
            Inventory = inventory;
            OppositeInventory = oppositeInventory;
        }
        
        public async Task<IReadOnlyCollection<LimitOrder>> CreateOrders()
        {
            var orders = new List<LimitOrder>();
            
            orders.AddRange(CreateOrders(InitialPrice, TradeType.Sell));
            orders.AddRange(CreateOrders(InitialPrice, TradeType.Buy));

            await _limitOrderStore.ClearAndAddOrders(orders);
            await _limitOrderStore.MarkOrdersDisabled(!_isEnabled);

            await _lykkeExchange.ApplyAsync(AssetPairId, orders);

            foreach (var order in orders)
            {
                await _limitOrderStore.PersistOrder(order);
            }
            
            orders.Clear();

            return orders;
        }

        public async Task<(IReadOnlyCollection<LimitOrder> addedOrders, IReadOnlyCollection<LimitOrder> removedOrders)>
            HandleTrades(IReadOnlyCollection<Trade> trades, decimal minVolume)
        {
            var addedOrders = new List<LimitOrder>();
            var removedOrders = new List<LimitOrder>();
            
            foreach (var trade in trades)
            {
                await HandleTrade(trade, minVolume);
                //addedOrders.AddRange(addedOrdersFromTrade);
                //removedOrders.AddRange(removedOrdersFromTrade);
            }

            return (addedOrders, removedOrders);
        }

        public async Task UpdateSettings(OrderBookTraderSettings settings)
        {
            IsEnabled = settings.IsEnabled;
            await _limitOrderStore.MarkOrdersDisabled(!_isEnabled);

            if (settings.InitialPrice != 0)
            {
                InitialPrice = settings.InitialPrice;
            }
            
            Delta = settings.Delta;
            Count = settings.Count;
            Volume = settings.Volume;    
        }

        public async Task<IReadOnlyCollection<LimitOrder>> GetOrders()
        {
            return await Task.FromResult(new List<LimitOrder>().AsReadOnly());
        }

        public async Task AddOrderManually([NotNull] LimitOrder limitOrder)
        {
            if (limitOrder == null) throw new ArgumentNullException(nameof(limitOrder));

            if (!string.Equals(limitOrder.AssetPairId, AssetPairId, StringComparison.InvariantCultureIgnoreCase))
                throw new ArgumentException("LimitOrder is for another AssetPair");
            
            await _limitOrderStore.AddSingleOrder(limitOrder);

            await _limitOrderStore.MarkOrdersDisabled(!_isEnabled);
        }

        public async Task<LimitOrder> CancelOrder(Guid orderId)
        {
            return await _limitOrderStore.RemoveSingleOrder(orderId);
        }

        public Task Clear()
        {
            return _limitOrderStore.Clear();
        }

        public Task RestoreOrders(IEnumerable<LimitOrder> limitOrders)
        {
            return _limitOrderStore.ClearAndAddOrders(limitOrders);
        }

        private async Task HandleTrade(Trade trade, decimal minVolume)
        {
            if (trade.Type == TradeType.None) throw new ArgumentException("Trade has None type", nameof(trade));

            await SpreadVolumeOnOrders(
                trade.Type == TradeType.Sell
                    ? (await _limitOrderStore.GetOrders(TradeType.Sell)).OrderBy(x => x.Price)
                    : (await _limitOrderStore.GetOrders(TradeType.Buy)).OrderByDescending(x => x.Price), 
                trade.Volume, minVolume);
        }
        
        private async Task SpreadVolumeOnOrders(IOrderedEnumerable<LimitOrder> orders, decimal volume, decimal minVolume)
        {
            foreach (var limitOrder in orders)
            {
                if (limitOrder.Volume <= volume || limitOrder.Volume - volume < minVolume)
                {
                    await _limitOrderStore.RemoveSingleOrder(limitOrder.Id);
                    //todo: mark order to make sure it's removed from ME later
                    await _lykkeExchange.CancelLimitOrderAsync(limitOrder.Id.ToString());
                    
                    var newOrder = CreateOppositeOrder(limitOrder);

                    await _limitOrderStore.AddSingleOrder(newOrder);

                    await _lykkeExchange.PlaceLimitOrderAsync(newOrder);

                    await _limitOrderStore.PersistOrder(newOrder);
                    
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

                    volume -= limitOrder.Volume;
                    
                    if (volume <= 0)
                    {
                        break;
                    }
                }
                else
                {
                    limitOrder.Volume -= volume;

                    await _limitOrderStore.PersistOrder(limitOrder);
                    
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
        }
        
        private IEnumerable<LimitOrder> CreateOrders(decimal initialPrice, TradeType tradeType)
        {
            decimal price = initialPrice;
            
            for (int i = 0; i < Count; i++)
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
        
        private LimitOrder CreateOppositeOrder(LimitOrder executedOrder)
        {
            return executedOrder.TradeType == TradeType.Sell
                ? new LimitOrder(SubtractDelta(executedOrder.Price), Volume, TradeType.Buy, AssetPairId, executedOrder.Number - 1)
                : new LimitOrder(AddDelta(executedOrder.Price), Volume, TradeType.Sell, AssetPairId, executedOrder.Number + 1);
        }
    }
}

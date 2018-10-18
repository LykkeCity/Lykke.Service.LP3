using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Settings;
using LimitOrder = Lykke.Service.LP3.Domain.Orders.LimitOrder;

namespace Lykke.Service.LP3.Domain.TradingAlgorithm
{
    public class OrderBookTrader
    {
        public string AssetPairId { get; }
        
        public decimal LevelVolumeSell { get; private set; }
        public decimal LevelVolumeBuy { get; private set; }
        public decimal LevelDelta { get; private set; }
        public decimal LevelOriginalVolume { get; private set; }
        public decimal ReferencePrice { get; private set; }

        public decimal Inventory { get; private set; }
        public decimal OppositeInventory { get; private set; }

        public bool IsEnabled { get; private set; }
        
        public decimal AdditionalOrdersDelta { get; private set; }
        public decimal AdditionalOrdersVolume { get; private set; }
        public int AdditionalOrdersCount { get; private set; }


        public OrderBookTrader(OrderBookTraderSettings settings)
        {
            AssetPairId = settings.AssetPairId;
            IsEnabled = settings.IsEnabled;
            ReferencePrice = settings.ReferencePrice;
            
            LevelDelta = settings.LevelDelta;
            LevelOriginalVolume = settings.LevelOriginalVolume;
            LevelVolumeBuy = settings.LevelOriginalVolume;
            LevelVolumeSell = -settings.LevelOriginalVolume;
            
            AdditionalOrdersDelta = settings.AdditionalOrdersDelta;
            AdditionalOrdersVolume = settings.AdditionalOrdersVolume;
            AdditionalOrdersCount = settings.AdditionalOrdersCount;
        }
        
        [UsedImplicitly] // used by Mapper
        public OrderBookTrader(string assetPairId, bool isEnabled, decimal referencePrice, 
            decimal levelDelta, decimal levelOriginalVolume, decimal levelVolumeBuy, decimal levelVolumeSell, 
            decimal additionalOrdersDelta, decimal additionalOrdersVolume, int additionalOrdersCount, 
            decimal inventory, decimal oppositeInventory)
        {
            AssetPairId = assetPairId;
            IsEnabled = isEnabled;
            ReferencePrice = referencePrice;
            
            LevelDelta = levelDelta;
            LevelOriginalVolume = levelOriginalVolume;
            LevelVolumeBuy = levelVolumeBuy;
            LevelVolumeSell = levelVolumeSell;
            
            AdditionalOrdersDelta = additionalOrdersDelta;
            AdditionalOrdersVolume = additionalOrdersVolume;
            AdditionalOrdersCount = additionalOrdersCount;
            
            Inventory = inventory;
            OppositeInventory = oppositeInventory;
        }
        
        private decimal Sell => (decimal) Math.Exp(Math.Log((double) ReferencePrice) + (double) LevelDelta);
        private decimal Buy => (decimal) Math.Exp(Math.Log((double) ReferencePrice) - (double) LevelDelta);
        
        private readonly AdditionalOrdersGenerator _additionalOrdersGenerator = new AdditionalOrdersGenerator();

        public IReadOnlyCollection<LimitOrder> GetOrders()
        {
            var levelOrders = GetLevelOrders();
            var additionalOrders = _additionalOrdersGenerator.GetOrders(levelOrders,
                AdditionalOrdersCount, AdditionalOrdersVolume, AdditionalOrdersDelta);

            var orders = levelOrders.Union(additionalOrders).ToList();

            MarkOrdersIfDisabled(orders);

            return orders;
        }

        private void MarkOrdersIfDisabled(List<LimitOrder> orders)
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

        private IReadOnlyCollection<LimitOrder> GetLevelOrders()
        {
            var sellOrder = new LimitOrder(Sell, Math.Abs(LevelVolumeSell), TradeType.Sell)
            {
                AssetPairId = AssetPairId
            };
            
            var buyOrder = new LimitOrder(Buy, LevelVolumeBuy, TradeType.Buy)
            {
                AssetPairId = AssetPairId
            };
            
            return new[] { sellOrder, buyOrder };
        }

        public void HandleTrades(IReadOnlyCollection<Trade> trades)
        {
            decimal volume = trades.Select(x => x.Type == TradeType.Sell ? -x.Volume : x.Volume).Sum();
            
            if (volume < 0)
            {
                HandleSellVolume(volume);
            }

            if (volume > 0)
            {
                HandleBuyVolume(volume);
            }
        }
        
        private void HandleSellVolume(decimal volume)
        {
            while (volume != 0)
            {
                if (volume <= LevelVolumeSell)
                {
                    volume -= LevelVolumeSell;

                    Inventory += LevelVolumeSell;
                    OppositeInventory -= LevelVolumeSell * Sell; // TODO: get rounded price from trade ? 

                    ReferencePrice = Sell;
                    LevelVolumeSell = -LevelOriginalVolume;
                }
                else
                {
                    LevelVolumeSell -= volume;

                    Inventory += volume;
                    OppositeInventory -= volume * Sell;

                    volume = 0;
                }
            }
        }

        private void HandleBuyVolume(decimal volume)
        {
            while (volume != 0)
            {
                if (volume >= LevelVolumeBuy)
                {
                    volume -= LevelVolumeBuy;

                    Inventory += LevelVolumeBuy;
                    OppositeInventory -= LevelVolumeBuy * Buy;

                    ReferencePrice = Buy;
                    LevelVolumeBuy = LevelOriginalVolume;
                }
                else
                {
                    LevelVolumeBuy -= volume;
                    Inventory += volume;
                    OppositeInventory -= volume * Buy;
                    volume = 0;
                }
            }
        }

        public void UpdateSettings(OrderBookTraderSettings settings)
        {
            IsEnabled = settings.IsEnabled;

            if (settings.ReferencePrice != 0)
            {
                ReferencePrice = settings.ReferencePrice;
            }
            
            LevelDelta = settings.LevelDelta;
            
            AdditionalOrdersDelta = settings.AdditionalOrdersDelta;
            AdditionalOrdersVolume = settings.AdditionalOrdersVolume;
            AdditionalOrdersCount = settings.AdditionalOrdersCount;
            
            if (settings.LevelOriginalVolume != LevelOriginalVolume)
            {
                LevelVolumeSell = -settings.LevelOriginalVolume;
                LevelVolumeBuy = settings.LevelOriginalVolume;
                LevelOriginalVolume = settings.LevelOriginalVolume;    
            }
        }
    }
}

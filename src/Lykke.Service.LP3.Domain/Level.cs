using System;
using System.Collections.Generic;
using Lykke.Service.LP3.Domain.Assets;
using Lykke.Service.LP3.Domain.Orders;

namespace Lykke.Service.LP3.Domain
{
    public class Level
    {
        public Level(string name, decimal delta, decimal volume)
        {
            Name = name;
            Delta = delta;
            
            OriginalVolume = volume;
            VolumeSell = -volume;
            VolumeBuy = volume;
        }

        public Level(string name, decimal delta, decimal volume, decimal volumeBuy, decimal volumeSell, 
            decimal inventory, decimal oppositeInventory, decimal reference, Guid sellOrderId, Guid buyOrderId)
            : this(name, delta, volume)
        {
            VolumeBuy = volumeBuy;
            VolumeSell = volumeSell;
            Inventory = inventory;
            OppositeInventory = oppositeInventory;
            Reference = reference;
            SellOrderId = sellOrderId;
            BuyOrderId = buyOrderId;
        }

        public decimal VolumeSell { get; set; }
        public decimal VolumeBuy { get; set; }
        public decimal Delta { get; private set; }
        public decimal Reference { get; private set; }
        public decimal OriginalVolume { get; private set; }
        public string Name { get; }

        public decimal Inventory { get; set; }
        public decimal OppositeInventory { get; set; }
        
        public Guid SellOrderId { get; private set; } = Guid.NewGuid();
        public Guid BuyOrderId { get; private set; } = Guid.NewGuid();

        public decimal Sell => (decimal) Math.Exp(Math.Log((double) Reference) + (double) Delta);
        public decimal Buy => (decimal) Math.Exp(Math.Log((double) Reference) - (double) Delta);

        public IEnumerable<LimitOrder> GetOrders(AssetPairInfo assetPairInfo)
        {
            var sellOrder = new LimitOrder(SellOrderId, Sell, Math.Abs(VolumeSell), TradeType.Sell) {LevelName = Name};
            sellOrder.Round(assetPairInfo);

            var buyOrder = new LimitOrder(BuyOrderId, Buy, VolumeBuy, TradeType.Buy) {LevelName = Name};
            buyOrder.Round(assetPairInfo);
            
            return new[] { sellOrder, buyOrder };
        }
        
        public void UpdateReference(decimal price)
        {
            Reference = price;
        }
        
        public void UpdateSettings(decimal delta, decimal volume)
        {
            Delta = delta;

            if (volume != OriginalVolume)
            {
                VolumeSell = -volume;
                VolumeBuy = volume;
                OriginalVolume = volume;    
            }
        }

        public decimal HandleSellVolume(decimal volume)
        {
            if (volume <= VolumeSell)
            {
                volume -= VolumeSell;

                Inventory += VolumeSell;
                OppositeInventory -= VolumeSell * Sell; // TODO: get rounded price from trade ? 

                Reference = Sell;
                VolumeSell = -OriginalVolume;
                
                SellOrderId = Guid.NewGuid();
            }
            else
            {
                VolumeSell -= volume;

                Inventory += volume;
                OppositeInventory -= volume * Sell;

                volume = 0;
            }

            return volume;
        }

        public decimal HandleBuyVolume(decimal volume)
        {
            if (volume >= VolumeBuy)
            {
                volume -= VolumeBuy;

                Inventory += VolumeBuy;
                OppositeInventory -= VolumeBuy * Buy;

                Reference = Buy;
                VolumeBuy = OriginalVolume;
                
                BuyOrderId = Guid.NewGuid();
            }
            else
            {
                VolumeBuy -= volume;
                Inventory += volume;
                OppositeInventory -= volume * Buy;
                volume = 0;
            }

            return volume;
        }
    }
}

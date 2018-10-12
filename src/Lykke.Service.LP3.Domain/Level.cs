using System;
using System.Collections.Generic;
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
            decimal inventory, decimal oppositeInventory,
            decimal reference)
         : this(name, delta, volume)
        {
            VolumeBuy = volumeBuy;
            VolumeSell = volumeSell;
            Inventory = inventory;
            OppositeInventory = oppositeInventory;
            Reference = reference;
        }

        public decimal VolumeSell { get; set; }
        public decimal VolumeBuy { get; set; }
        public decimal Delta { get; private set; }
        public decimal Reference { get; private set; }
        public decimal OriginalVolume { get; private set; }
        public string Name { get; }

        public decimal Inventory { get; set; }
        public decimal OppositeInventory { get; set; }

        public decimal Sell => (decimal) Math.Exp(Math.Log((double) Reference) + (double) Delta);
        public decimal Buy => (decimal) Math.Exp(Math.Log((double) Reference) - (double) Delta);

        public IEnumerable<LimitOrder> GetOrders()
        {
            return new[]
            {
                new LimitOrder(_sellOrderId, Sell, Math.Abs(VolumeSell), TradeType.Sell), 
                new LimitOrder(_buyOrderId, Buy, VolumeBuy, TradeType.Buy) 
            };
        }

        private Guid _sellOrderId = Guid.NewGuid();  // TODO: persistent
        private Guid _buyOrderId = Guid.NewGuid();

        public void UpdateReference(decimal price)
        {
            Reference = price;
            
            _sellOrderId = Guid.NewGuid();
            _buyOrderId = Guid.NewGuid();
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
    }
}

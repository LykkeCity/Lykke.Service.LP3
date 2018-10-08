using System;
using System.Collections.Generic;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Settings;
using Lykke.Service.LP3.Domain.States;

namespace Lykke.Service.LP3.Domain
{
    public class Level
    {
        public Level(LevelSettings levelSettings)
        {
            OriginalVolume = levelSettings.Volume;
            Delta = levelSettings.Delta;
            Name = levelSettings.Name;
            VolumeSell = -levelSettings.Volume;
            VolumeBuy = levelSettings.Volume;
        }

        public Level(LevelSettings levelSettings, LevelState levelState)
         : this(levelSettings)
        {
            if (levelState.VolumeBuy != 0)
            {
                VolumeBuy = levelState.VolumeBuy;
            }

            if (levelState.VolumeSell != 0)
            {
                VolumeSell = levelState.VolumeSell;
            }
            
            //Reference = levelState.Reference;
            Inventory = levelState.Inventory;
            OppositeInventory = levelState.OppositeInventory;
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
                new LimitOrder(Sell, Math.Abs(VolumeSell), TradeType.Sell), 
                new LimitOrder(Buy, VolumeBuy, TradeType.Buy), 
            };
        }

        public void UpdateReference(decimal price)
        {
            Reference = price;
        }
        
        public void UpdateSettings(LevelSettings levelSettings)
        {
            Delta = levelSettings.Delta;
            VolumeSell = -levelSettings.Volume;
            VolumeBuy = levelSettings.Volume;
            OriginalVolume = levelSettings.Volume;
        }
    }
}

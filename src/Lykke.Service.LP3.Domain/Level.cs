using System;
using System.Collections.Generic;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Settings;

namespace Lykke.Service.LP3.Domain
{
    public class Level
    {
        public Level(LevelSettings levelSettings, decimal startMid)
        {
            VolumeSell = -levelSettings.Volume;
            VolumeBuy = levelSettings.Volume;
            OriginalVolume = levelSettings.Volume;
            Delta = levelSettings.Delta;
            Reference = startMid;
            Name = levelSettings.Name;
        }

        public decimal VolumeSell { get; set; }
        public decimal VolumeBuy { get; set; }
        public decimal Delta { get; }
        public decimal Reference { get; set; }
        public decimal OriginalVolume { get; }
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
    }
}

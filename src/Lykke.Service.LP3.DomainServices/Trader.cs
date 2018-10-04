using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.LP3.Domain;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Services;

namespace Lykke.Service.LP3.DomainServices
{
    public class Trader : ITrader
    {
        private readonly ISettingsService _settingsService;
        private List<Level> _levels = new List<Level>();
        
        private decimal _inventory = 0;
        private decimal _oppositeInventory = 0;
        
        public Trader(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task StartAsync(decimal startMid)
        {
            _levels = (await _settingsService.GetLevelSettingsAsync()).Select(x => new Level(x, startMid)).ToList();
        }
        
        public IEnumerable<LimitOrder> GetOrders()
        {
            return _levels.SelectMany(x => x.GetOrders());
        }

        public async Task HandleTradeAsync(Trade trade)
        {
            var volume = trade.Volume;

            if (trade.Type == TradeType.Sell)
            {
                volume *= -1;
            }
            
            while (volume != 0)
            {
                volume = HandleVolume(volume);
            }
        }
        
        private decimal HandleVolume(decimal volume)
        {
            if (volume < 0)
            {
                var level = _levels.OrderBy(e => e.Sell).First();

                if (volume <= level.VolumeSell)
                {
                    volume -= level.VolumeSell;

                    _inventory += level.VolumeSell;
                    _oppositeInventory -= level.VolumeSell * level.Sell;

                    level.Inventory += level.VolumeSell;
                    level.OppositeInventory -= level.VolumeSell * level.Sell;

                    level.Reference = level.Sell;
                    level.VolumeSell = -level.OriginalVolume;
                }
                else
                {
                    level.VolumeSell -= volume;

                    _inventory += volume;
                    _oppositeInventory -= volume * level.Sell;
                    level.Inventory += volume;
                    level.OppositeInventory -= volume * level.Sell;

                    volume = 0;
                }

                return volume;
            }

            if (volume > 0)
            {
                var level = _levels.OrderByDescending(e => e.Buy).First();

                if (volume >= level.VolumeBuy)
                {
                    volume -= level.VolumeBuy;

                    _inventory += level.VolumeBuy;
                    _oppositeInventory -= level.VolumeBuy * level.Buy;

                    level.Inventory += level.VolumeBuy;
                    level.OppositeInventory -= level.VolumeBuy * level.Buy;

                    level.Reference = level.Buy;
                    level.VolumeBuy = level.OriginalVolume;
                }
                else
                {
                    level.VolumeBuy -= volume;
                    _inventory += volume;
                    _oppositeInventory -= volume * level.Buy;
                    level.Inventory += volume;
                    level.OppositeInventory -= volume * level.Buy;
                    volume = 0;
                }
                return volume;
            }

            return 0;
        }
    }
}

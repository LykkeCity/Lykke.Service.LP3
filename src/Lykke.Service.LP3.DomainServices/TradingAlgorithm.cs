using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.LP3.Domain;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Services;
using Lykke.Service.LP3.Domain.Settings;

namespace Lykke.Service.LP3.DomainServices
{
    public class TradingAlgorithm : ITradingAlgorithm
    {
        private readonly ILevelsService _levelsService;
        private List<Level> _levels = new List<Level>();
        
        private decimal _inventory = 0;
        private decimal _oppositeInventory = 0;
        private readonly ILog _log;
        private decimal _lastPrice;

        public TradingAlgorithm(ILogFactory logFactory,
            ILevelsService levelsService)
        {
            _levelsService = levelsService;
            _log = logFactory.CreateLog(this);

            levelsService.SettingsChanged += OnSettingsChanged;
        }

        public async Task StartAsync(decimal startMid)
        {
            _levels = (await _levelsService.GetLevels()).ToList();
            
            foreach (var level in _levels)
            {
                level.UpdateReference(startMid);
            }
        }
        
        public IEnumerable<LimitOrder> GetOrders()
        {
            return _levels.SelectMany(x => x.GetOrders());
        }

        public IReadOnlyList<Level> GetLevels()
        {
            return _levels;
        }

        public void HandleTrade(Trade trade)
        {
            _log.Info("Trade is received", context: $"Trade: {trade.ToJson()}");

            _lastPrice = trade.Price;
            var volume = trade.Volume;

            if (trade.Type == TradeType.Sell)
            {
                volume *= -1;
            }
            
            while (volume != 0)
            {
                volume = HandleVolume(volume);
            }
            
            _levelsService.SaveStatesAsync(_levels).GetAwaiter().GetResult();
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

                    level.UpdateReference(level.Sell);
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
                
                _log.Info($"Level {level.Name} is executed", context: $"Level state: {level.ToJson()}");

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

                    level.UpdateReference(level.Buy);
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
                
                _log.Info($"Level {level.Name} is executed", context: $"Level state: {level.ToJson()}");
                
                return volume;
            }

            return 0;
        }

        private void OnSettingsChanged(LevelsChangedEventArgs eventArgs)
        {
            if (eventArgs.AddedLevel != null)
            {
                var level = new Level(eventArgs.AddedLevel);
                level.UpdateReference(_lastPrice);
                _levels.Add(level);
            }

            if (eventArgs.NameOfDeletedLevel != null)
            {
                var level = _levels.SingleOrDefault(x =>
                    string.Equals(x.Name, eventArgs.NameOfDeletedLevel, StringComparison.InvariantCultureIgnoreCase));

                _levels.Remove(level);
            }

            if (eventArgs.ChangedLevel != null)
            {
                var level = _levels.SingleOrDefault(x =>
                    string.Equals(x.Name, eventArgs.ChangedLevel.Name, StringComparison.InvariantCultureIgnoreCase));
                
                level?.UpdateSettings(eventArgs.ChangedLevel);
            }
        }
    }
}

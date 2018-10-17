using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.LP3.Domain;
using Lykke.Service.LP3.Domain.Assets;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Repositories;
using Lykke.Service.LP3.Domain.Services;

namespace Lykke.Service.LP3.DomainServices
{
    public class LevelsService : ILevelsService, IStartable
    {
        private readonly ILevelRepository _levelRepository;

        private List<Level> _levels;
        private decimal _lastPrice;
        private readonly ILog _log;

        public LevelsService(
            ILogFactory logFactory,
            ILevelRepository levelRepository)
        {
            _levelRepository = levelRepository;
            _log = logFactory.CreateLog(this);
        }

        public async Task AddAsync(Level level)
        {
            level.UpdateReference(_lastPrice);
            
            await _levelRepository.AddAsync(level);
            
            _levels.Add(level);
            
            _log.Info("New level was added", 
                context: $"Added level: {level.ToJson()}, {GetCurrentLevelsLogString()}");
        }

        public async Task DeleteAsync(string name)
        {
            await _levelRepository.DeleteAsync(name);

            var level = FindLevelByName(name);

            _levels.Remove(level);
            
            _log.Info("Level was deleted", 
                context: $"Deleted level: {level.ToJson()}, {GetCurrentLevelsLogString()}");
        }

        public async Task UpdateAsync(string name, decimal delta, decimal volume)
        {
            var level = FindLevelByName(name);
            if (level == null)
            {
                _log.Warning($"Trying to update settings of non-existing level {name}", context: GetCurrentLevelsLogString());
                return;
            }
            
            level.UpdateSettings(delta, volume);

            await SaveStatesAsync();
            
            _log.Info("Level was updated", 
                context: $"Updated level: {level.ToJson()}, {GetCurrentLevelsLogString()}");
        }

        public IReadOnlyList<Level> GetLevels()
        {
            return _levels;
        }
        
        public IEnumerable<LimitOrder> GetOrders(AssetPairInfo assetPairInfo)
        {
            return _levels.SelectMany(x => x.GetOrders(assetPairInfo));
        }
        
        private Level FindLevelByName(string name)
        {
            return _levels.SingleOrDefault(x => string.Equals(x.Name, name, StringComparison.InvariantCultureIgnoreCase));
        }

        public void UpdateReference(decimal lastPrice, bool force = false)
        {
            _lastPrice = lastPrice;
            
            foreach (var level in _levels.Where(x => force || x.Reference == 0))
            {
                level.UpdateReference(lastPrice);
            }
        }

        private async Task PopulateLevels()
        {
            if (_levels == null)
            {
                _levels = (await _levelRepository.GetLevels()).ToList();
                
                _log.Info("Levels are populated from repository", 
                    context: $"Current levels: [{string.Join(", ", _levels.Select(x => x.ToJson()))}]");
            }
        }

        public async Task SaveStatesAsync()
        {
            if (_levels != null)
            {
                await _levelRepository.SaveStatesAsync(_levels);
            }
        }

        public void Start()
        {
            PopulateLevels().GetAwaiter().GetResult();
        }

        private string GetCurrentLevelsLogString()
        {
            return $"current levels: [{string.Join(", ", _levels.Select(x => x.ToJson()))}]";
        }
    }
}

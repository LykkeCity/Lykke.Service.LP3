using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Lykke.Service.LP3.Domain;
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

        public LevelsService(ILevelRepository levelRepository)
        {
            _levelRepository = levelRepository;
        }

        public async Task AddAsync(Level level)
        {
            level.UpdateReference(_lastPrice);
            
            await _levelRepository.AddAsync(level);
            
            _levels.Add(level);
        }

        public async Task DeleteAsync(string name)
        {
            await _levelRepository.DeleteAsync(name);

            var level = FindLevelByName(name);

            _levels.Remove(level);
        }

        public async Task UpdateAsync(string name, decimal delta, decimal volume)
        {
            await _levelRepository.UpdateSettingsAsync(name, delta, volume);

            var level = FindLevelByName(name);
            level?.UpdateSettings(delta, volume);
        }

        public IReadOnlyList<Level> GetLevels()
        {
            return _levels;
        }
        
        public IEnumerable<LimitOrder> GetOrders()
        {
            return _levels.SelectMany(x => x.GetOrders());
        }
        
        private Level FindLevelByName(string name)
        {
            return _levels.SingleOrDefault(x => string.Equals(x.Name, name, StringComparison.InvariantCultureIgnoreCase));
        }

        public void UpdateReference(decimal lastPrice)
        {
            _lastPrice = lastPrice;
            
            foreach (var level in _levels.Where(x => x.Reference == 0))
            {
                level.UpdateReference(lastPrice);
            }
        }

        private async Task PopulateLevels()
        {
            if (_levels == null)
            {
                _levels = (await _levelRepository.GetLevels()).ToList();
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
    }
}

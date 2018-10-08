using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Domain;
using Lykke.Service.LP3.Domain.Repositories;
using Lykke.Service.LP3.Domain.Services;
using Lykke.Service.LP3.Domain.Settings;
using Lykke.Service.LP3.Domain.States;

namespace Lykke.Service.LP3.DomainServices
{
    public class LevelsService : ILevelsService
    {
        public event Action<LevelsChangedEventArgs> SettingsChanged;
        
        private readonly ILevelRepository _levelRepository;


        public LevelsService(ILevelRepository levelRepository)
        {
            _levelRepository = levelRepository;
        }

        public async Task AddAsync(LevelSettings levelSettings)
        {
            await _levelRepository.AddSettingsAsync(levelSettings);
            SettingsChanged?.Invoke(new LevelsChangedEventArgs
            {
                AddedLevel = levelSettings
            });
        }

        public async Task DeleteAsync(string name)
        {
            await _levelRepository.DeleteAsync(name);
            SettingsChanged?.Invoke(new LevelsChangedEventArgs
            {
                NameOfDeletedLevel = name
            });
        }

        public async Task UpdateAsync(LevelSettings levelSettings)
        {
            await _levelRepository.UpdateSettingsAsync(levelSettings);
            SettingsChanged?.Invoke(new LevelsChangedEventArgs
            {
                ChangedLevel = levelSettings
            });
        }

        public Task<IReadOnlyList<Level>> GetLevels()
        {
            return _levelRepository.GetLevels();
        }

        public Task SaveStatesAsync(IEnumerable<Level> levels)
        {
            return _levelRepository.SaveStatesAsync(levels);
        }

        public Task<IReadOnlyList<LevelSettings>> GetLevelSettingsAsync()
        {
            return _levelRepository.GetSettingsAsync();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Domain.Repositories;
using Lykke.Service.LP3.Domain.Services;
using Lykke.Service.LP3.Domain.Settings;

namespace Lykke.Service.LP3.DomainServices
{
    public class SettingsService : ISettingsService
    {
        private readonly string _walletId;
        private readonly ILevelsSettingsRepository _levelsSettingsRepository;
        private readonly IBaseAssetPairSettingsRepository _baseAssetPairSettingsRepository;

        public event Action<SettingsChangedEventArgs> SettingsChanged;
        
        // TODO: settingsCache

        public SettingsService(string walletId,
            ILevelsSettingsRepository levelsSettingsRepository,
            IBaseAssetPairSettingsRepository baseAssetPairSettingsRepository)
        {
            _walletId = walletId;
            _levelsSettingsRepository = levelsSettingsRepository;
            _baseAssetPairSettingsRepository = baseAssetPairSettingsRepository;
        }


        public Task<string> GetWalletIdAsync()
        {
            return Task.FromResult(_walletId);
        }

        public Task<BaseAssetPairSettings> GetBaseAssetPairSettings()
        {
            return _baseAssetPairSettingsRepository.GetAsync();
        }

        public async Task AddAsync(LevelSettings levelSettings)
        {
            await _levelsSettingsRepository.AddAsync(levelSettings);
            SettingsChanged?.Invoke(new SettingsChangedEventArgs
            {
                AddedLevel = levelSettings
            });
        }

        public async Task DeleteAsync(string name)
        {
            await _levelsSettingsRepository.DeleteAsync(name);
            SettingsChanged?.Invoke(new SettingsChangedEventArgs
            {
                NameOfDeletedLevel = name
            });
        }

        public async Task UpdateAsync(LevelSettings levelSettings)
        {
            await _levelsSettingsRepository.UpdateAsync(levelSettings);
            SettingsChanged?.Invoke(new SettingsChangedEventArgs
            {
                ChangedLevel = levelSettings
            });
        }

        public Task<IReadOnlyList<LevelSettings>> GetLevelSettingsAsync()
        {
            return _levelsSettingsRepository.GetAsync();
        }
    }
}

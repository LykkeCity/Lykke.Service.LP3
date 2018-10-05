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

        // TODO: settingsCache

        public SettingsService(string walletId,
            ILevelsSettingsRepository levelsSettingsRepository)
        {
            _walletId = walletId;
            _levelsSettingsRepository = levelsSettingsRepository;
        }

        public Task<string> GetWalletIdAsync()
        {
            return Task.FromResult(_walletId);
        }

        public Task<string> GetBaseAssetPairId()
        {
            throw new NotImplementedException();
        }

        public Task AddAsync(LevelSettings levelSettings)
        {
            return _levelsSettingsRepository.AddAsync(levelSettings);
        }

        public Task DeleteAsync(string name)
        {
            return _levelsSettingsRepository.DeleteAsync(name);
        }

        public Task UpdateAsync(LevelSettings levelSettings)
        {
            return _levelsSettingsRepository.UpdateAsync(levelSettings);
        }

        public Task<IReadOnlyList<LevelSettings>> GetLevelSettingsAsync()
        {
            return _levelsSettingsRepository.GetAsync();
        }
    }
}

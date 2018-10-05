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

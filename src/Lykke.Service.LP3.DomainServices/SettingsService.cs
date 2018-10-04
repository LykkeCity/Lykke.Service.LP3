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
        private readonly ISettingsRepository _settingsRepository;

        // settingsCache

        public SettingsService(string walletId,
            ISettingsRepository settingsRepository)
        {
            _walletId = walletId;
            _settingsRepository = settingsRepository;
        }

        public Task<string> GetWalletIdAsync()
        {
            return Task.FromResult(_walletId);
        }

        public Task<string> GetBaseAssetPairId()
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<LevelSettings>> GetLevelSettingsAsync()
        {
            return _settingsRepository.GetLevelSettingsAsync();
        }
    }
}

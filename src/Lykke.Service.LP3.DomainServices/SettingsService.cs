using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.RemoteUi;
using Lykke.Service.LP3.Domain.Repositories;
using Lykke.Service.LP3.Domain.Services;
using Lykke.Service.LP3.Domain.Settings;
using Lykke.Service.LP3.Domain.States;

namespace Lykke.Service.LP3.DomainServices
{
    public class SettingsService : ISettingsService
    {
        private readonly string _walletId;
        private readonly ILevelRepository _levelRepository;
        private readonly IBaseAssetPairSettingsRepository _baseAssetPairSettingsRepository;

        public event Action<LevelsChangedEventArgs> SettingsChanged;
        
        // TODO: settingsCache

        public SettingsService(string walletId,
            ILevelRepository levelRepository,
            IBaseAssetPairSettingsRepository baseAssetPairSettingsRepository)
        {
            _walletId = walletId;
            _levelRepository = levelRepository;
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
    }
}

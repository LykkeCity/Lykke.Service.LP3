using System.Threading.Tasks;
using Lykke.Service.LP3.Domain.Repositories;
using Lykke.Service.LP3.Domain.Services;
using Lykke.Service.LP3.Domain.Settings;

namespace Lykke.Service.LP3.DomainServices
{
    public class SettingsService : ISettingsService
    {
        private readonly string _walletId;
        private readonly IBaseAssetPairSettingsRepository _baseAssetPairSettingsRepository;
        private readonly IAdditionalVolumeSettingsRepository _additionalVolumeSettingsRepository;

        public SettingsService(string walletId,
            IBaseAssetPairSettingsRepository baseAssetPairSettingsRepository,
            IAdditionalVolumeSettingsRepository additionalVolumeSettingsRepository)
        {
            _walletId = walletId;
            _baseAssetPairSettingsRepository = baseAssetPairSettingsRepository;
            _additionalVolumeSettingsRepository = additionalVolumeSettingsRepository;
        }

        public Task<string> GetWalletIdAsync()
        {
            return Task.FromResult(_walletId);
        }

        public Task<BaseAssetPairSettings> GetBaseAssetPairSettings()
        {
            return _baseAssetPairSettingsRepository.GetAsync();
        }

        public Task SaveBaseAssetPairSettings(BaseAssetPairSettings settings)
        {
            return _baseAssetPairSettingsRepository.AddOrUpdateAsync(settings);
        }
        
        public Task UpdateAdditionalVolumeSettingsAsync(AdditionalVolumeSettings settings)
        {
            return _additionalVolumeSettingsRepository.AddOrUpdateAsync(settings);
        }

        public Task<AdditionalVolumeSettings> GetAdditionalVolumeSettingsAsync()
        {
            return _additionalVolumeSettingsRepository.GetAsync();
        }
    }
}

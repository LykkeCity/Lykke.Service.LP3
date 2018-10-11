using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.LP3.Domain.Repositories;
using Lykke.Service.LP3.Domain.Services;
using Lykke.Service.LP3.Domain.Settings;

namespace Lykke.Service.LP3.DomainServices
{
    public class SettingsService : ISettingsService
    {
        private readonly string _walletId;
        private readonly IAssetPairSettingsRepository _assetPairSettingsRepository;
        private readonly IAdditionalVolumeSettingsRepository _additionalVolumeSettingsRepository;
        private readonly List<string> _availableExternalExchanges;
        private readonly ILog _log;

        public SettingsService(string walletId,
            ILogFactory logFactory,
            IAssetPairSettingsRepository assetPairSettingsRepository,
            IAdditionalVolumeSettingsRepository additionalVolumeSettingsRepository,
            IEnumerable<string> availableExternalExchanges)
        {
            _walletId = walletId;
            _assetPairSettingsRepository = assetPairSettingsRepository;
            _additionalVolumeSettingsRepository = additionalVolumeSettingsRepository;
            _availableExternalExchanges = availableExternalExchanges?.ToList() ?? new List<string>();
            _log = logFactory.CreateLog(this);
        }

        public string GetWalletId()
        {
            return _walletId;
        }

        public IReadOnlyList<string> GetAvailableExternalExchanges()
        {
            return _availableExternalExchanges;
        }

        public Task<AssetPairSettings> GetBaseAssetPairSettings()
        {
            return _assetPairSettingsRepository.GetBaseAsync();
        }

        public async Task SaveBaseAssetPairSettings(AssetPairSettings settings)
        {
            await _assetPairSettingsRepository.AddOrUpdateBaseAsync(settings);
            _log.Info("Base asset settings were updated", context: $"new settings: {settings.ToJson()}");
        }

        public async Task UpdateDependentAssetPairSettingsAsync(AssetPairSettings settings)
        {
            await _assetPairSettingsRepository.AddOrUpdateDependentAsync(settings);
            _log.Info("Dependent asset settings were updated", context: $"new settings: {settings.ToJson()}");
        }

        public async Task<IEnumerable<AssetPairSettings>> GetDependentAssetPairsSettingsAsync()
        {
            return await _assetPairSettingsRepository.GetDependentAsync();
        }

        public Task DeleteDependentAssetPairSettingsAsync(string assetPairId)
        {
            return _assetPairSettingsRepository.DeleteDependentAsync(assetPairId);
        }

        public async Task UpdateAdditionalVolumeSettingsAsync(AdditionalVolumeSettings settings)
        {
            await _additionalVolumeSettingsRepository.AddOrUpdateAsync(settings);
            _log.Info("Additional volume settings were updated", context: $"new settings: {settings.ToJson()}");
        }

        public Task<AdditionalVolumeSettings> GetAdditionalVolumeSettingsAsync()
        {
            return _additionalVolumeSettingsRepository.GetAsync();
        }
    }
}

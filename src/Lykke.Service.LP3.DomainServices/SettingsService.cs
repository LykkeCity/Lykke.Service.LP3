using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.LP3.Domain;
using Lykke.Service.LP3.Domain.Repositories;
using Lykke.Service.LP3.Domain.Services;
using Lykke.Service.LP3.Domain.Settings;
using Lykke.Service.LP3.DomainServices.Caches;

namespace Lykke.Service.LP3.DomainServices
{
    public class SettingsService : ISettingsService
    {
        private readonly string _walletId;
        private readonly IAssetPairSettingsRepository _assetPairSettingsRepository;
        private readonly IAdditionalVolumeSettingsRepository _additionalVolumeSettingsRepository;
        private readonly IInitialPriceRepository _initialPriceRepository;
        private readonly List<string> _availableExternalExchanges;
        private readonly ILog _log;
        
        private readonly DependentAssetPairSettingsCache _dependentAssetPairSettingsCache = new DependentAssetPairSettingsCache();

        public SettingsService(string walletId,
            ILogFactory logFactory,
            IAssetPairSettingsRepository assetPairSettingsRepository,
            IAdditionalVolumeSettingsRepository additionalVolumeSettingsRepository,
            IInitialPriceRepository initialPriceRepository,
            IEnumerable<string> availableExternalExchanges)
        {
            _walletId = walletId;
            _assetPairSettingsRepository = assetPairSettingsRepository;
            _additionalVolumeSettingsRepository = additionalVolumeSettingsRepository;
            _initialPriceRepository = initialPriceRepository;
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

        public async Task DeleteBaseAssetPairSettingsAsync()
        {
            await _assetPairSettingsRepository.DeleteBaseAsync();
            await _initialPriceRepository.DeleteAsync();
            _log.Info("Base asset pair was deleted. Initial price was deleted as well.");
        }

        public Task<AssetPairSettings> GetBaseAssetPairSettingsAsync()
        {
            return _assetPairSettingsRepository.GetBaseAsync();
        }

        public async Task SaveBaseAssetPairSettingsAsync(AssetPairSettings settings)
        {
            await _assetPairSettingsRepository.AddOrUpdateBaseAsync(settings);
            _log.Info("Base asset settings were updated", context: $"new settings: {settings.ToJson()}");
        }
        
        
        public Task<InitialPrice> GetInitialPriceAsync()
        {
            return _initialPriceRepository.GetAsync();
        }

        public Task AddOrUpdateInitialPriceAsync(decimal price)
        {
            return _initialPriceRepository.AddOrUpdateAsync(price);
        }

        public Task DeleteInitialPriceAsync()
        {
            return _initialPriceRepository.DeleteAsync();
        }
        

        public async Task UpdateDependentAssetPairSettingsAsync(AssetPairSettings settings)
        {
            await _assetPairSettingsRepository.AddOrUpdateDependentAsync(settings);
            _log.Info("Dependent asset settings were updated", context: $"new settings: {settings.ToJson()}");
            
            _dependentAssetPairSettingsCache.Clear();
        }

        public async Task<IEnumerable<AssetPairSettings>> GetDependentAssetPairsSettingsAsync()
        {
            if (!_dependentAssetPairSettingsCache.Initialized)
            {
                _dependentAssetPairSettingsCache.Init(await _assetPairSettingsRepository.GetDependentAsync());   
            }
            
            return _dependentAssetPairSettingsCache.GetAll();
        }

        public async Task DeleteDependentAssetPairSettingsAsync(string assetPairId)
        {
            await _assetPairSettingsRepository.DeleteDependentAsync(assetPairId);

            _dependentAssetPairSettingsCache.Clear();
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

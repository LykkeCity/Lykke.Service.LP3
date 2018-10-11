using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Domain.Settings;

namespace Lykke.Service.LP3.Domain.Services
{
    public interface ISettingsService
    {
        string GetWalletId();

        Task<AssetPairSettings> GetBaseAssetPairSettings();
        Task SaveBaseAssetPairSettings(AssetPairSettings settings);

        Task UpdateDependentAssetPairSettingsAsync(AssetPairSettings settings);
        Task<IEnumerable<AssetPairSettings>> GetDependentAssetPairsSettingsAsync();
        Task DeleteDependentAssetPairSettingsAsync(string assetPairId);

        Task UpdateAdditionalVolumeSettingsAsync(AdditionalVolumeSettings settings);
        Task<AdditionalVolumeSettings> GetAdditionalVolumeSettingsAsync();
        IReadOnlyList<string> GetAvailableExternalExchanges();
    }
}

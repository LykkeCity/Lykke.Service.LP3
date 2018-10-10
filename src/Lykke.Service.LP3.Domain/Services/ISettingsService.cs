using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Domain.Settings;

namespace Lykke.Service.LP3.Domain.Services
{
    public interface ISettingsService
    {
        string GetWalletId();

        Task<BaseAssetPairSettings> GetBaseAssetPairSettings();
        Task SaveBaseAssetPairSettings(BaseAssetPairSettings settings);
        
        Task UpdateAdditionalVolumeSettingsAsync(AdditionalVolumeSettings settings);
        Task<AdditionalVolumeSettings> GetAdditionalVolumeSettingsAsync();
        IReadOnlyList<string> GetAvailableExternalExchanges();
    }
}

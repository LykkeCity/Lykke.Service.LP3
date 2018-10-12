using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Client.Models.Settings;
using Refit;

namespace Lykke.Service.LP3.Client
{
    public interface ISettingsApi
    {
        [Get("/api/settings/baseAssetPair")]
        Task<BaseAssetPairSettingsModel> GetBaseAssetPairSettingsAsync();
        
        [Post("/api/settings/baseAssetPair")]
        Task SaveBaseAssetPairSettingsAsync(BaseAssetPairSettingsModel model);

        [Delete("/api/settings/baseAssetPair")]
        Task DeleteBaseAssetPairSettingsAsync();

        
        
        [Get("/api/settings/dependentAssetPairs")]
        Task<IReadOnlyList<AssetPairSettingsModel>> GetDependentAssetPairSettingsAsync();
        
        [Put("/api/settings/dependentAssetPairs")]
        Task UpdateDependentAssetPairSettingsAsync(AssetPairSettingsModel model);
        
        [Delete("/api/settings/dependentAssetPairs")]
        Task DeleteDependentAssetPairSettingsAsync(string assetPairId);
        
        
        
        [Get("/api/settings/levels")]
        Task<IReadOnlyList<LevelSettingsModel>> GetLevelsSettingsAsync();

        [Post("/api/settings/levels")]
        Task AddAsync(LevelSettingsModel model);

        [Delete("/api/settings/levels/{name}")]
        Task DeleteAsync(string name);

        [Put("/api/settings/levels")]
        Task UpdateAsync(LevelSettingsModel model);
        
        

        [Get("/api/settings/additionalVolumeSettings")]
        Task<AdditionalVolumeSettingsModel> GetAdditionalVolumeSettingsAsync();

        [Post("/api/settings/additionalVolumeSettings")]
        Task UpdateAdditionalVolumeSettingsAsync(AdditionalVolumeSettingsModel model);
        
        

        [Get("/api/settings/walletId")]
        string GetWalletId();

        [Get("/api/settings/availableExchanges")]
        IReadOnlyList<string> GetAvailableExchanges();
    }
}

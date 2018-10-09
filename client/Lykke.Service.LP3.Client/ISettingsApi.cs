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
    }
}

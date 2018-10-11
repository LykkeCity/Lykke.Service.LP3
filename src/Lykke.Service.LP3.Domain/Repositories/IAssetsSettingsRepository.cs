using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Domain.Settings;

namespace Lykke.Service.LP3.Domain.Repositories
{
    public interface IAssetPairSettingsRepository
    {
        Task<AssetPairSettings> GetBaseAsync();
        Task AddOrUpdateBaseAsync(AssetPairSettings settings);
        Task DeleteBaseAsync();
        
        Task<IReadOnlyList<AssetPairSettings>> GetDependentAsync();
        Task AddOrUpdateDependentAsync(AssetPairSettings settings);
        Task DeleteDependentAsync(string assetPairId);
    }
}

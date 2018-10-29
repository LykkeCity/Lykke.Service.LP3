using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Domain.Assets;

namespace Lykke.Service.LP3.Domain.Services
{
    public interface IAssetsService
    {
        AssetPairInfo GetAssetPairInfo(string assetPairId);

        AssetInfo GetAssetInfo(string assetId);


        Task<IReadOnlyList<AssetPairInfo>> GetAllAsync();
        Task AddAssetPairInfoAsync(AssetPairInfo assetPairInfo);
        Task UpdateAssetPairInfoAsync(AssetPairInfo assetPairInfo);
        Task DeleteAssetPairInfoAsync(string assetPairId);
    }
}

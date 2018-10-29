using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Client.Models.Assets;
using Refit;

namespace Lykke.Service.LP3.Client
{
    public interface IAssetPairsApi
    {
        [Get("/api/assetPairs/{assetPairId}")]
        Task<AssetPairInfoModel> GetAsync(string assetPairId);

        [Post("/api/assetPairs")]
        Task AddAsync(AssetPairInfoModel model);
        
        [Put("/api/assetPairs")]
        Task UpdateAsync(AssetPairInfoModel model);

        [Get("/api/assetPairs")]
        Task<IReadOnlyList<AssetPairInfoModel>> GetAllAsync();
    }
}

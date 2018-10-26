using System.Threading.Tasks;
using Lykke.Service.LP3.Client.Models.Assets;
using Refit;

namespace Lykke.Service.LP3.Client
{
    public interface IAssetPairsApi
    {
        [Get("/api/assetPairs/{assetPairId}")]
        Task<AssetPairInfoModel> GetAsync(string assetPairId);
    }
}

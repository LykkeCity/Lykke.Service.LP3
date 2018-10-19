using Lykke.Service.LP3.Domain.Assets;

namespace Lykke.Service.LP3.Domain.Services
{
    public interface IAssetsService
    {
        AssetPairInfo GetAssetPairInfo(string assetPairId);

        AssetInfo GetAssetInfo(string assetId);
    }
}

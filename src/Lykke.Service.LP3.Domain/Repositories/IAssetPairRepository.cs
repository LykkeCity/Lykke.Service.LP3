using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Domain.Assets;

namespace Lykke.Service.LP3.Domain.Repositories
{
    public interface IAssetPairRepository
    {
        Task AddAsync(AssetPairInfo assetPair);
        Task<IReadOnlyList<AssetPairInfo>> GetAllAsync();
        Task UpdateAsync(AssetPairInfo assetPair);
        Task DeleteAsync(string assetPairId);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.LP3.Domain.Assets;
using Lykke.Service.LP3.Domain.Repositories;
using Lykke.Service.LP3.Domain.Services;

namespace Lykke.Service.LP3.DomainServices
{
    public class AssetsService : IAssetsService
    {
        private readonly IAssetPairRepository _assetPairRepository;

        private readonly Dictionary<string, AssetPairInfo> _assetPairInfos =
            new Dictionary<string, AssetPairInfo>(StringComparer.InvariantCultureIgnoreCase);

        public AssetsService(IAssetPairRepository assetPairRepository)
        {
            _assetPairRepository = assetPairRepository;
        }
        
        public AssetPairInfo GetAssetPairInfo(string assetPairId)
        {
            if (!_assetPairInfos.Any())
            {
                var assetPairInfos =  _assetPairRepository.GetAllAsync().GetAwaiter().GetResult();

                foreach (var assetPairInfo in assetPairInfos)
                {
                    _assetPairInfos.TryAdd(assetPairInfo.AssetPairId, assetPairInfo);
                }
            }


            if (_assetPairInfos.TryGetValue(assetPairId, out var result))
            {
                return result;
            }

            return null;
        }

        public Task<IReadOnlyList<AssetPairInfo>> GetAllAsync()
        {
            return _assetPairRepository.GetAllAsync();
        }

        public Task AddAssetPairInfoAsync(AssetPairInfo assetPairInfo)
        {
            return _assetPairRepository.AddAsync(assetPairInfo);
        }

        public Task UpdateAssetPairInfoAsync(AssetPairInfo assetPairInfo)
        {
            return _assetPairRepository.UpdateAsync(assetPairInfo);
        }

        public Task DeleteAssetPairInfoAsync(string assetPairId)
        {
            return _assetPairRepository.DeleteAsync(assetPairId);
        }

        public AssetInfo GetAssetInfo(string assetId)
        {
            throw new NotImplementedException();
        }
    }
}

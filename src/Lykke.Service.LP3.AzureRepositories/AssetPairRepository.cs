using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.LP3.Domain.Assets;
using Lykke.Service.LP3.Domain.Repositories;

namespace Lykke.Service.LP3.AzureRepositories
{
    public class AssetPairRepository : IAssetPairRepository
    {
        private readonly INoSQLTableStorage<AssetPairEntity> _storage;

        public AssetPairRepository(INoSQLTableStorage<AssetPairEntity> storage)
        {
            _storage = storage;
        }
        
        public async Task AddAsync(AssetPairInfo assetPair)
        {
            var entity = new AssetPairEntity(GetPartitionKey(), GetRowKey(assetPair.AssetPairId));

            Mapper.Map(assetPair, entity);

            await _storage.InsertAsync(entity);
        }
        
        public async Task<IReadOnlyList<AssetPairInfo>> GetAllAsync()
        {
            IEnumerable<AssetPairEntity> data = await _storage.GetDataAsync(GetPartitionKey());

            return Mapper.Map<List<AssetPairInfo>>(data);
        }

        public async Task UpdateAsync(AssetPairInfo assetPair)
        {
            var entity = new AssetPairEntity(GetPartitionKey(), GetRowKey(assetPair.AssetPairId));

            Mapper.Map(assetPair, entity);

            await _storage.InsertOrReplaceAsync(entity);
        }

        public async Task DeleteAsync(string assetPairId)
        {
            await _storage.DeleteAsync(GetPartitionKey(), GetRowKey(assetPairId));
        }

        private string GetPartitionKey() => "AssetPairs";

        private string GetRowKey(string assetPairId) => assetPairId.ToUpperInvariant();
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.LP3.Domain.Repositories;
using Lykke.Service.LP3.Domain.Settings;

namespace Lykke.Service.LP3.AzureRepositories
{
    public class AssetPairSettingsRepository : IAssetPairSettingsRepository
    {
        private readonly INoSQLTableStorage<AssetPairSettingsEntity> _storage;

        public AssetPairSettingsRepository(INoSQLTableStorage<AssetPairSettingsEntity> storage)
        {
            _storage = storage;
        }

        public async Task<AssetPairSettings> GetBaseAsync()
        {
            var data = await _storage.GetDataAsync(GetPartitionKeyForBase(), GetRowKeyForBase());

            var settings = Mapper.Map<AssetPairSettings>(data);

            return settings;
        }

        public Task AddOrUpdateBaseAsync(AssetPairSettings settings)
        {
            var entity = new AssetPairSettingsEntity(GetPartitionKeyForBase(), GetRowKeyForBase());
            Mapper.Map(settings, entity);

            return _storage.InsertOrReplaceAsync(entity);
        }

        public Task DeleteBaseAsync()
        {
            return _storage.DeleteAsync(GetPartitionKeyForBase(), GetRowKeyForBase());
        }

        public async Task<IReadOnlyList<AssetPairSettings>> GetDependentAsync()
        {
            var data = await _storage.GetDataAsync(GetPartitionKeyForDependent());

            var settings = Mapper.Map<List<AssetPairSettings>>(data);

            return settings;
        }

        public Task AddOrUpdateDependentAsync(AssetPairSettings settings)
        {
            var entity = new AssetPairSettingsEntity(GetPartitionKeyForDependent(), GetRowKeyForDependent(settings.AssetPairId));
            Mapper.Map(settings, entity);

            return _storage.InsertOrReplaceAsync(entity);
        }

        public Task DeleteDependentAsync(string assetPairId)
        {
            return _storage.DeleteAsync(GetPartitionKeyForDependent(), GetRowKeyForDependent(assetPairId));
        }

        private static string GetPartitionKeyForBase() => "BaseAssetPairSettings";

        private static string GetRowKeyForBase() => "BaseAssetPairSettings";

        private static string GetPartitionKeyForDependent() => "DependentAssetPairSettings";

        private static string GetRowKeyForDependent(string assetPairId) => assetPairId.ToLowerInvariant();
    }
}

using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.LP3.Domain.Repositories;
using Lykke.Service.LP3.Domain.Settings;

namespace Lykke.Service.LP3.AzureRepositories
{
    public class BaseAssetPairSettingsRepository : IBaseAssetPairSettingsRepository
    {
        private readonly INoSQLTableStorage<BaseAssetPairSettingsEntity> _storage;

        public BaseAssetPairSettingsRepository(INoSQLTableStorage<BaseAssetPairSettingsEntity> storage)
        {
            _storage = storage;
        }

        public async Task<BaseAssetPairSettings> GetAsync()
        {
            var data = await _storage.GetDataAsync(GetPartitionKey(), GetRowKey());

            var settings = Mapper.Map<BaseAssetPairSettings>(data);

            return settings;
        }

        public Task AddOrUpdateAsync(string baseAssetPairId)
        {
            var entity = new BaseAssetPairSettingsEntity(GetPartitionKey(), GetRowKey())
            {
                AssetPairId = baseAssetPairId
            };

            return _storage.InsertOrReplaceAsync(entity);
        }

        public Task DeleteAsync()
        {
            return _storage.DeleteAsync(GetPartitionKey(), GetRowKey());
        }

        private static string GetPartitionKey() => "BaseAssetPairSettings";

        private static string GetRowKey() => "BaseAssetPairSettings";
    }
}

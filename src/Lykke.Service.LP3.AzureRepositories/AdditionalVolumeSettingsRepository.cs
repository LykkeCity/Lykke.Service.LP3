using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.LP3.Domain.Repositories;
using Lykke.Service.LP3.Domain.Settings;

namespace Lykke.Service.LP3.AzureRepositories
{
    public class AdditionalVolumeSettingsRepository : IAdditionalVolumeSettingsRepository
    {
        private readonly INoSQLTableStorage<AdditionalVolumeSettingsEntity> _storage;

        public AdditionalVolumeSettingsRepository(INoSQLTableStorage<AdditionalVolumeSettingsEntity> storage)
        {
            _storage = storage;
        }

        public Task AddOrUpdateAsync(AdditionalVolumeSettings settings)
        {
            var entity = new AdditionalVolumeSettingsEntity(GetPartitionKey(), GetRowKey());
            Mapper.Map(settings, entity);

            return _storage.InsertOrReplaceAsync(entity);
        }

        public async Task<AdditionalVolumeSettings> GetAsync()
        {
            var entity = await _storage.GetDataAsync(GetPartitionKey(), GetRowKey());

            return Mapper.Map<AdditionalVolumeSettings>(entity);
        }
        
        private static string GetPartitionKey() => "AdditionalVolumeSettings";

        private static string GetRowKey() => "AdditionalVolumeSettings";
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.LP3.Domain.Repositories;
using Lykke.Service.LP3.Domain.Settings;

namespace Lykke.Service.LP3.AzureRepositories
{
    public class LevelsSettingsRepository : ILevelsSettingsRepository
    {
        private readonly INoSQLTableStorage<LevelSettingsEntity> _storage;

        public LevelsSettingsRepository(INoSQLTableStorage<LevelSettingsEntity> storage)
        {
            _storage = storage;
        }

        public async Task<IReadOnlyList<LevelSettings>> GetAsync()
        {
            var data = await _storage.GetDataAsync(GetPartitionKey());

            return Mapper.Map<List<LevelSettings>>(data);
        }

        public Task AddAsync(LevelSettings levelSettings)
        {
            var entity = new LevelSettingsEntity(GetPartitionKey(), GetRowKey(levelSettings.Name));
            Mapper.Map(levelSettings, entity);

            return _storage.InsertAsync(entity);
        }

        public Task DeleteAsync(string name)
        {
            return _storage.DeleteAsync(GetPartitionKey(), GetRowKey(name));
        }

        public Task UpdateAsync(LevelSettings levelSettings)
        {
            var entity = new LevelSettingsEntity(GetPartitionKey(), GetRowKey(levelSettings.Name));
            Mapper.Map(levelSettings, entity);
            entity.ETag = "*";

            return _storage.ReplaceAsync(entity); 
        }

        private static string GetPartitionKey()
            => "LevelsSettings";

        private static string GetRowKey(string name)
            => name.GetHashCode().ToString();
    }
}

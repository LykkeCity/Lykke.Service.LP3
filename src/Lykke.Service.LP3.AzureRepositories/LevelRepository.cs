using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.LP3.Domain;
using Lykke.Service.LP3.Domain.Repositories;
using Lykke.Service.LP3.Domain.Settings;
using Lykke.Service.LP3.Domain.States;

namespace Lykke.Service.LP3.AzureRepositories
{
    public class LevelRepository : ILevelRepository
    {
        private readonly INoSQLTableStorage<LevelEntity> _storage;

        public LevelRepository(INoSQLTableStorage<LevelEntity> storage)
        {
            _storage = storage;
        }

        public async Task<IReadOnlyList<LevelSettings>> GetSettingsAsync()
        {
            var data = await _storage.GetDataAsync(GetPartitionKey());

            return Mapper.Map<List<LevelSettings>>(data);
        }

        public Task AddSettingsAsync(LevelSettings levelSettings)
        {
            var entity = new LevelEntity(GetPartitionKey(), GetRowKey(levelSettings.Name));
            Mapper.Map(levelSettings, entity);

            return _storage.InsertAsync(entity);
        }

        public Task DeleteAsync(string name)
        {
            return _storage.DeleteAsync(GetPartitionKey(), GetRowKey(name));
        }

        public Task UpdateSettingsAsync(LevelSettings levelSettings)
        {
            var entity = new LevelEntity(GetPartitionKey(), GetRowKey(levelSettings.Name));
            Mapper.Map(levelSettings, entity);

            return _storage.InsertOrMergeAsync(entity);
        }

        public async Task<IReadOnlyList<Level>> GetLevels()
        {
            var data = await _storage.GetDataAsync(GetPartitionKey());

            var result = new List<Level>();

            foreach (var levelEntity in data)
            {
                var settings = Mapper.Map<LevelSettings>(levelEntity);
                var state = Mapper.Map<LevelState>(levelEntity);
                
                result.Add(new Level(settings, state));
            }
            
            return result;
        }

        public Task SaveStatesAsync(IEnumerable<Level> levels)
        {
            var entities = levels.Select(level =>
            {
                var entity = new LevelEntity(GetPartitionKey(), GetRowKey(level.Name));
                Mapper.Map(level, entity);
                return entity;
            });
            
            return _storage.InsertOrMergeBatchAsync(entities);
        }

        private static string GetPartitionKey()
            => "LevelsSettings";

        private static string GetRowKey(string name)
            => name.GetHashCode().ToString();
    }
}

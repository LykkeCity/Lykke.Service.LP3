using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.LP3.Domain;
using Lykke.Service.LP3.Domain.Repositories;

namespace Lykke.Service.LP3.AzureRepositories
{
    public class LevelRepository : ILevelRepository
    {
        private readonly INoSQLTableStorage<LevelEntity> _storage;

        public LevelRepository(INoSQLTableStorage<LevelEntity> storage)
        {
            _storage = storage;
        }

        public Task AddAsync(Level level)
        {
            var entity = new LevelEntity(GetPartitionKey(), GetRowKey(level.Name));
            Mapper.Map(level, entity);

            return _storage.InsertAsync(entity);
        }

        public Task DeleteAsync(string name)
        {
            return _storage.DeleteAsync(GetPartitionKey(), GetRowKey(name));
        }

        public async Task<IReadOnlyList<Level>> GetLevels()
        {
            var data = await _storage.GetDataAsync(GetPartitionKey());

            var result = Mapper.Map<List<Level>>(data);
            
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
            => name.ToLowerInvariant();
    }
}

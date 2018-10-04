using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.LP3.Domain.Repositories;
using Lykke.Service.LP3.Domain.Settings;

namespace Lykke.Service.LP3.AzureRepositories
{
    public class SettingsRepository : ISettingsRepository
    {
        private readonly INoSQLTableStorage<LevelSettingsEntity> _storage;

        public SettingsRepository(INoSQLTableStorage<LevelSettingsEntity> storage)
        {
            _storage = storage;
        }

        public async Task<IReadOnlyList<LevelSettings>> GetLevelSettingsAsync()
        {
            var data = await _storage.GetDataAsync(GetPartitionKey());

            return Mapper.Map<List<LevelSettings>>(data);
        }
        
        private static string GetPartitionKey()
            => "Settings";

        private static string GetRowKey()
            => "Settings";
    }
}

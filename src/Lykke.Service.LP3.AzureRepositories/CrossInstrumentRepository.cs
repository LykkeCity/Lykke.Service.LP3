using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.LP3.Domain.CrossInstruments;
using Lykke.Service.LP3.Domain.Repositories;

namespace Lykke.Service.LP3.AzureRepositories
{
    public class CrossInstrumentRepository : ICrossInstrumentRepository
    {
        private readonly INoSQLTableStorage<CrossInstrumentEntity> _storage;

        public CrossInstrumentRepository(INoSQLTableStorage<CrossInstrumentEntity> storage)
        {
            _storage = storage;
        }

        public async Task<IReadOnlyList<CrossInstrument>> GetAsync()
        {
            var data = await _storage.GetDataAsync(GetPartitionKey());

            return Mapper.Map<List<CrossInstrument>>(data);
        }

        public Task AddAsync(CrossInstrument crossInstrument)
        {
            var entity = new CrossInstrumentEntity(GetPartitionKey(), GetRowKey(crossInstrument.Exchange, crossInstrument.AssetPairId));
            Mapper.Map(crossInstrument, entity);

            return _storage.InsertAsync(entity);
        }

        public Task UpdateAsync(CrossInstrument crossInstrument)
        {
            var entity = new CrossInstrumentEntity(GetPartitionKey(), GetRowKey(crossInstrument.Exchange, crossInstrument.AssetPairId))
            {
                ETag = "*"
            };
            Mapper.Map(crossInstrument, entity);

            return _storage.ReplaceAsync(entity);
        }

        public Task DeleteAsync(string exchange, string assetPairId)
        {
            return _storage.DeleteAsync(GetPartitionKey(), GetRowKey(exchange, assetPairId));
        }

        public async Task<CrossInstrument> GetAsync(string exchange, string assetPairId)
        {
            var data = await _storage.GetDataAsync(GetPartitionKey(), GetRowKey(exchange, assetPairId));

            return Mapper.Map<CrossInstrument>(data);
        }

        private static string GetPartitionKey() => "CrossInstruments";

        private static string GetRowKey(string exchange, string assetPairId) => 
            $"{exchange}_{assetPairId}".ToLowerInvariant();
        
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.LP3.Domain;
using Lykke.Service.LP3.Domain.Repositories;

namespace Lykke.Service.LP3.AzureRepositories
{
    public class LastTickPriceRepository : ILastTickPriceRepository
    {
        private readonly INoSQLTableStorage<LastTickPriceEntity> _storage;

        public LastTickPriceRepository(INoSQLTableStorage<LastTickPriceEntity> storage)
        {
            _storage = storage;
        }
        
        public async Task<IReadOnlyList<TickPrice>> GetAllAsync()
        {
            var data = await _storage.GetDataAsync();

            return Mapper.Map<List<TickPrice>>(data);
        }

        public Task AddOrUpdateAsync(TickPrice tickPrice)
        {
            var entity = new LastTickPriceEntity(GetPartitionKey(tickPrice.Source), GetRowKey(tickPrice.AssetPair));
            Mapper.Map(tickPrice, entity);

            return _storage.InsertOrReplaceAsync(entity);
        }

        private string GetPartitionKey(string source) => source.ToLowerInvariant();

        private string GetRowKey(string assetPair) => assetPair.ToLowerInvariant();
    }
}

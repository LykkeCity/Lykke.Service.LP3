using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.LP3.Domain;
using Lykke.Service.LP3.Domain.Repositories;

namespace Lykke.Service.LP3.AzureRepositories
{
    public class InitialPriceRepository : IInitialPriceRepository
    {
        private readonly INoSQLTableStorage<InitialPriceEntity> _storage;

        public InitialPriceRepository(INoSQLTableStorage<InitialPriceEntity> storage)
        {
            _storage = storage;
        }

        public async Task<InitialPrice> GetAsync()
        {
            var data = await _storage.GetDataAsync(GetPartitionKey(), GetRowKey());

            return data == null ? null : new InitialPrice(data.Price);
        }

        public Task AddOrUpdateAsync(decimal price)
        {
            var entity = new InitialPriceEntity(GetPartitionKey(), GetRowKey())
            {
                Price = price
            };

            return _storage.InsertOrReplaceAsync(entity);
        }

        public Task DeleteAsync()
        {
            return _storage.DeleteAsync(GetPartitionKey(), GetRowKey());
        }

        private static string GetPartitionKey() => "InitialPrice";

        private static string GetRowKey() => "InitialPrice";
    }
}

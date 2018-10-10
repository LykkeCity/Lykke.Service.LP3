using AzureStorage;
using Lykke.Service.LP3.Domain.Repositories;

namespace Lykke.Service.LP3.AzureRepositories
{
    public class CrossRateRepository : ICrossRateRepository
    {
        private readonly INoSQLTableStorage<CrossRateEntity> _storage;

        public CrossRateRepository(INoSQLTableStorage<CrossRateEntity> storage)
        {
            _storage = storage;
        }
    }
}

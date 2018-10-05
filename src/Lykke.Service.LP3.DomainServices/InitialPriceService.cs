using System.Threading.Tasks;
using Lykke.Service.LP3.Domain;
using Lykke.Service.LP3.Domain.Repositories;
using Lykke.Service.LP3.Domain.Services;

namespace Lykke.Service.LP3.DomainServices
{
    public class InitialPriceService : IInitialPriceService
    {
        private readonly IInitialPriceRepository _initialPriceRepository;

        public InitialPriceService(IInitialPriceRepository initialPriceRepository)
        {
            _initialPriceRepository = initialPriceRepository;
        }

        public Task<InitialPrice> GetAsync()
        {
            return _initialPriceRepository.GetAsync();
        }

        public Task AddOrUpdateAsync(decimal price)
        {
            return _initialPriceRepository.AddOrUpdateAsync(price);
        }

        public Task DeleteAsync()
        {
            return _initialPriceRepository.DeleteAsync();
        }
    }
}

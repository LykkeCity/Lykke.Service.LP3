using System.Threading.Tasks;

namespace Lykke.Service.LP3.Domain.Repositories
{
    public interface IInitialPriceRepository
    {
        Task<InitialPrice> GetAsync();
        Task AddOrUpdateAsync(decimal price);
        Task DeleteAsync();
    }
}

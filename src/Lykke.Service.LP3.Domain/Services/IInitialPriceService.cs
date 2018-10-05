using System.Threading.Tasks;

namespace Lykke.Service.LP3.Domain.Services
{
    public interface IInitialPriceService
    {
        Task<InitialPrice> GetAsync();
        Task AddOrUpdateAsync(decimal price);
        Task DeleteAsync();
    }
}

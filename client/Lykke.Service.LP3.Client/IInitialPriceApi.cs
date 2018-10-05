using System.Threading.Tasks;
using Lykke.Service.LP3.Client.Models;
using Refit;

namespace Lykke.Service.LP3.Client
{
    public interface IInitialPriceApi
    {
        [Get("/api/initialPrice")]
        Task<InitialPriceModel> GetAsync();
        
        [Post("/api/initialPrice")]
        Task AddOrUpdateAsync(InitialPriceModel model);
        
        [Delete("/api/initialPrice")]
        Task DeleteAsync();
    }
}

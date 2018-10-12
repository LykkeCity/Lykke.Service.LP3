using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Client.Models;
using Refit;

namespace Lykke.Service.LP3.Client
{
    public interface ICrossTickPricesApi
    {
        [Get("/api/crossTickPrices")]
        Task<IReadOnlyList<TickPriceModel>> GetAllAsync();
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.LP3.Domain.Repositories
{
    public interface ILastTickPriceRepository
    {
        Task<IReadOnlyList<TickPrice>> GetAllAsync();
        Task AddOrUpdateAsync(TickPrice tickPrice);
    }
}

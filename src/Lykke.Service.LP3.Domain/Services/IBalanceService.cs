using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.LP3.Domain.Services
{
    public interface IBalanceService
    {
        Task<IReadOnlyCollection<Balance>> GetAllAsync();

        Task<Balance> GetByAssetIdAsync(string assetId);

        Task UpdateBalancesAsync();
    }
}

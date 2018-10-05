using System.Threading.Tasks;
using Lykke.Service.LP3.Domain.Settings;

namespace Lykke.Service.LP3.Domain.Repositories
{
    public interface IBaseAssetPairSettingsRepository
    {
        Task<BaseAssetPairSettings> GetAsync();
        Task AddOrUpdateAsync(string baseAssetPairId);
        Task DeleteAsync();
    }
}

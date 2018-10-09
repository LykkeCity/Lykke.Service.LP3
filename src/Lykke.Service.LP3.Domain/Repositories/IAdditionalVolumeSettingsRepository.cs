using System.Threading.Tasks;
using Lykke.Service.LP3.Domain.Settings;

namespace Lykke.Service.LP3.Domain.Repositories
{
    public interface IAdditionalVolumeSettingsRepository
    {
        Task AddOrUpdateAsync(AdditionalVolumeSettings settings);
        Task<AdditionalVolumeSettings> GetAsync();
    }
}

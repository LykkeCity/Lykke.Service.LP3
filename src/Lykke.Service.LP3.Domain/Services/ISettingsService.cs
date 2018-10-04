using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Domain.Settings;

namespace Lykke.Service.LP3.Domain.Services
{
    public interface ISettingsService
    {
        Task<string> GetWalletIdAsync();

        Task<IReadOnlyList<LevelSettings>> GetLevelSettingsAsync();

        Task<string> GetBaseAssetPairId();
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Domain.Settings;

namespace Lykke.Service.LP3.Domain.Repositories
{
    public interface ISettingsRepository
    {
        Task<IReadOnlyList<LevelSettings>> GetLevelSettingsAsync();
    }
}

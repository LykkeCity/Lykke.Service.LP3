using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Domain.Settings;

namespace Lykke.Service.LP3.Domain.Repositories
{
    public interface ILevelRepository
    {
        Task<IReadOnlyList<LevelSettings>> GetSettingsAsync();
        Task AddSettingsAsync(LevelSettings levelSettings);
        Task DeleteAsync(string name);
        Task UpdateSettingsAsync(LevelSettings levelSettings);
        Task<IReadOnlyList<Level>> GetLevels();
        Task SaveStatesAsync(IEnumerable<Level> levels);
    }
}

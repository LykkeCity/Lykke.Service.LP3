using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Domain.Settings;

namespace Lykke.Service.LP3.Domain.Repositories
{
    public interface ILevelsSettingsRepository
    {
        Task<IReadOnlyList<LevelSettings>> GetAsync();
        Task AddAsync(LevelSettings levelSettings);
        Task DeleteAsync(string name);
        Task UpdateAsync(LevelSettings levelSettings);
    }
}

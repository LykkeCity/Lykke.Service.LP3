using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.LP3.Domain.Repositories
{
    public interface ILevelRepository
    {
        Task AddAsync(Level level);
        Task DeleteAsync(string name);
        Task<IReadOnlyList<Level>> GetLevels();
        Task SaveStatesAsync(IEnumerable<Level> levels);
    }
}

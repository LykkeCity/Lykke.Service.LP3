using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Domain.Settings;

namespace Lykke.Service.LP3.Domain.Services
{
    public interface ILevelsService
    {
        event Action<LevelsChangedEventArgs> SettingsChanged;
        
        Task SaveStatesAsync(IEnumerable<Level> levels);

        Task<IReadOnlyList<LevelSettings>> GetLevelSettingsAsync();

        Task AddAsync(LevelSettings levelSettings);
        
        Task DeleteAsync(string name);
        
        Task UpdateAsync(LevelSettings levelSettings);

        Task<IReadOnlyList<Level>> GetLevels();
    }
}

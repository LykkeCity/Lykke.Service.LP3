using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Domain.Settings;

namespace Lykke.Service.LP3.Domain.Services
{
    public interface ISettingsService
    {
        event Action<SettingsChangedEventArgs> SettingsChanged;
        
        Task<string> GetWalletIdAsync();

        Task<IReadOnlyList<LevelSettings>> GetLevelSettingsAsync();

        Task<BaseAssetPairSettings> GetBaseAssetPairSettings();
        
        Task AddAsync(LevelSettings levelSettings);
        
        Task DeleteAsync(string name);
        
        Task UpdateAsync(LevelSettings levelSettings);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Client.Models.Settings;

namespace Lykke.Service.LP3.Client
{
    public interface ISettingsApi
    {
        Task<IReadOnlyList<LevelSettingsModel>> GetLevelsSettingsAsync();
    }
}

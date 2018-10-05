using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Client.Models.Settings;
using Refit;

namespace Lykke.Service.LP3.Client
{
    public interface ISettingsApi
    {
        [Get("levels")]
        Task<IReadOnlyList<LevelSettingsModel>> GetLevelsSettingsAsync();

        [Post("levels")]
        Task AddAsync(LevelSettingsModel model);

        [Delete("levels/{name}")]
        Task DeleteAsync(string name);

        [Put("levels")]
        Task UpdateAsync(LevelSettingsModel model);
    }
}

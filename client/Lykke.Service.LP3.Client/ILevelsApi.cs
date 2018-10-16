using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Client.Models.Levels;
using Refit;

namespace Lykke.Service.LP3.Client
{
    public interface ILevelsApi
    {
        [Get("/api/levels")]
        Task<IReadOnlyList<LevelModel>> GetAllAsync();
    }
}

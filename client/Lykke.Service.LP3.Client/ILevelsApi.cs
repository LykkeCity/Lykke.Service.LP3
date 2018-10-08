using System.Collections.Generic;
using Lykke.Service.LP3.Client.Models.Levels;
using Refit;

namespace Lykke.Service.LP3.Client
{
    public interface ILevelsApi
    {
        [Get("/api/levels")]
        IReadOnlyList<LevelModel> GetAll();
    }
}

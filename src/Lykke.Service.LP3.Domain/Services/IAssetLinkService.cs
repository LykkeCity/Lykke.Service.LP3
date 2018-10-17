using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Domain.Assets;

namespace Lykke.Service.LP3.Domain.Services
{
    public interface IAssetLinkService
    {
        Task<IReadOnlyCollection<AssetLink>> GetAllAsync();
    }
}

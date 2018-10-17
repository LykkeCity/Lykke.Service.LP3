using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Domain.Assets;
using Lykke.Service.LP3.Domain.Orders;

namespace Lykke.Service.LP3.Domain.Services
{
    public interface ILevelsService
    {
        Task SaveStatesAsync();

        Task AddAsync(Level level);
        
        Task DeleteAsync(string name);
        
        Task UpdateAsync(string name, decimal delta, decimal volume);

        IReadOnlyList<Level> GetLevels();
        
        void UpdateReference(decimal lastPrice, bool force = false);

        IEnumerable<LimitOrder> GetOrders(AssetPairInfo assetPairInfo);
    }
}

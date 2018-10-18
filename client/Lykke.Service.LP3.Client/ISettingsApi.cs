using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.LP3.Client.Models.Settings;
using Refit;

namespace Lykke.Service.LP3.Client
{
    public interface ISettingsApi
    {
        [Get("/api/settings/orderBookTraders")]
        Task<IReadOnlyList<OrderBookTraderSettingsModel>> GetOrderBookTradersSettingsAsync();
        
        [Put("/api/settings/orderBookTraders")]
        Task UpdateOrderBookTraderSettingsAsync(OrderBookTraderSettingsModel model);
                
        [Post("/api/settings/orderBookTraders")]
        Task AddOrderBookTraderAsync(OrderBookTraderSettingsModel model);
        
        [Delete("/api/settings/orderBookTraders")]
        Task DeleteOrderBookTraderAsync(string assetPairId);
        
        

        [Get("/api/settings/walletId")]
        Task<string> GetWalletIdAsync();
    }
}

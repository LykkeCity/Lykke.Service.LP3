using System.Threading.Tasks;
using Refit;

namespace Lykke.Service.LP3.Client
{
    public interface ISettingsApi
    {
        /// <summary>
        /// Get the current WalletId on which all the traders operate
        /// </summary>
        [Get("/api/settings/walletId")]
        Task<string> GetWalletIdAsync();
    }
}

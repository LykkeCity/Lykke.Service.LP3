using System.Threading.Tasks;
using Refit;

namespace Lykke.Service.LP3.Client
{
    public interface ISettingsApi
    {
        [Get("/api/settings/walletId")]
        Task<string> GetWalletIdAsync();
    }
}

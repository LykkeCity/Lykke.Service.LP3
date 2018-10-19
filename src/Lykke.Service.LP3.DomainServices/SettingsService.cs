using Lykke.Service.LP3.Domain.Services;

namespace Lykke.Service.LP3.DomainServices
{
    public class SettingsService : ISettingsService
    {
        private readonly string _walletId;
        
        public SettingsService(string walletId)
        {
            _walletId = walletId;
        }

        public string GetWalletId()
        {
            return _walletId;
        }
    }
}

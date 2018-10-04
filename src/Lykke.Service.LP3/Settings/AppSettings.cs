using JetBrains.Annotations;
using Lykke.Sdk.Settings;
using Lykke.Service.Assets.Client;
using Lykke.Service.Balances.Client;
using Lykke.Service.LP3.Settings.Clients;

namespace Lykke.Service.LP3.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public LP3Settings LP3Service { get; set; }
        
        public BalancesServiceClientSettings BalancesServiceClient { get; set; }
        
        public AssetServiceSettings AssetsServiceClient { get; set; }
        
        public MatchingEngineClientSettings MatchingEngineClient { get; set; }
    }
}

using Lykke.HttpClientGenerator;

namespace Lykke.Service.LP3.Client
{
    /// <summary>
    /// LP3 API aggregating interface.
    /// </summary>
    public class LP3Client : ILP3Client
    {
        /// <summary>API for get and change settings</summary>
        public ISettingsApi SettingsApi { get; }
        
        public IOrdersApi OrdersApi { get; }
        
        public ITradesApi TradesApi { get; }
        
        /// <summary>API for getting balances</summary>
        public IBalancesApi BalancesApi { get; }

        /// <summary>C-tor</summary>
        public LP3Client(IHttpClientGenerator httpClientGenerator)
        {
            SettingsApi = httpClientGenerator.Generate<ISettingsApi>();
            OrdersApi = httpClientGenerator.Generate<IOrdersApi>();
            TradesApi = httpClientGenerator.Generate<ITradesApi>();
            BalancesApi = httpClientGenerator.Generate<IBalancesApi>();
        }
    }
}

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
        
        /// <summary>API for set and remove initial price for the algorithm</summary>
        public IInitialPriceApi InitialPriceApi { get; }

        public IOrdersApi OrdersApi { get; }
        
        public ILevelsApi LevelsApi { get; }
        
        public ITradesApi TradesApi { get; }
        
        public ICrossTickPricesApi CrossTickPricesApi { get; }
        
        /// <summary>C-tor</summary>
        public LP3Client(IHttpClientGenerator httpClientGenerator)
        {
            SettingsApi = httpClientGenerator.Generate<ISettingsApi>();
            InitialPriceApi = httpClientGenerator.Generate<IInitialPriceApi>();
            OrdersApi = httpClientGenerator.Generate<IOrdersApi>();
            LevelsApi = httpClientGenerator.Generate<ILevelsApi>();
            TradesApi = httpClientGenerator.Generate<ITradesApi>();
            CrossTickPricesApi = httpClientGenerator.Generate<ICrossTickPricesApi>();
        }
    }
}

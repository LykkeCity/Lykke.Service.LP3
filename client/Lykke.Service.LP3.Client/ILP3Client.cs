using JetBrains.Annotations;

namespace Lykke.Service.LP3.Client
{
    /// <summary>
    /// LP3 client interface.
    /// </summary>
    [PublicAPI]
    public interface ILP3Client
    {
        /// <summary>API for get and change settings</summary>
        ISettingsApi SettingsApi { get; }
        
        /// <summary>API for set and remove initial price for the algorithm</summary>
        IInitialPriceApi InitialPriceApi { get; }
        
        IOrdersApi OrdersApi { get; }
        
        ILevelsApi LevelsApi { get; }
        
        ITradesApi TradesApi { get; }
    }
}

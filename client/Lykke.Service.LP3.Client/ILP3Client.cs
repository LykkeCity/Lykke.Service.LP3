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
        
        /// <summary>API for managing current orders</summary>
        IOrdersApi OrdersApi { get; }
        
        /// <summary>API for getting executed trades</summary>
        ITradesApi TradesApi { get; }
        
        /// <summary>API for getting current balances</summary>
        IBalancesApi BalancesApi { get; }
        
        /// <summary>API for managing traders</summary>
        IOrderBookTradersApi OrderBookTradersApi { get; }
    }
}

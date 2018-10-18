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
        
        IOrdersApi OrdersApi { get; }
        
        ITradesApi TradesApi { get; }
        
        IBalancesApi BalancesApi { get; }
    }
}

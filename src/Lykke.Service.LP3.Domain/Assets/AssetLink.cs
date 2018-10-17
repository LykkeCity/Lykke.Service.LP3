namespace Lykke.Service.LP3.Domain.Assets
{
    /// <summary>
    /// Represents a mapping between internal and external asset.
    /// </summary>
    public class AssetLink
    {
        /// <summary>
        /// The identifier of asset used for internal exchange.
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// The identifier of asset used for external exchange.
        /// </summary>
        public string ExternalAssetId { get; set; }
    }
}

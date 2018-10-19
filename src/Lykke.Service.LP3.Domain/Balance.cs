using System;

namespace Lykke.Service.LP3.Domain
{
    /// <summary>
    /// Represents a balance of an asset.
    /// </summary>
    public class Balance
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Balance"/> of asset with amount of balance.
        /// </summary>
        /// <param name="assetId">The asset id.</param>
        /// <param name="amount">The amount of balance.</param>
        /// <param name="reserved">The amount of reserved balance.</param>
        public Balance(string assetId, decimal amount, decimal reserved)
        {
            AssetId = assetId;
            Amount = amount;
            Reserved = reserved;
            Available = amount - reserved;
        }

        /// <summary>
        /// The asset id.
        /// </summary>
        public string AssetId { get; }

        /// <summary>
        /// The amount of balance.
        /// </summary>
        public decimal Amount { get; }

        /// <summary>
        /// The amount of reserved balance.
        /// </summary>
        public decimal Reserved { get; }

        /// <summary>
        /// The amount of available balance.
        /// </summary>
        public decimal Available { get; }
    }
}

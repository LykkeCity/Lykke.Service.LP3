using JetBrains.Annotations;

namespace Lykke.Service.LP3.Client.Models.Balances
{
    /// <summary>
    /// Represents an asset balance details.
    /// </summary>
    [PublicAPI]
    public class AssetBalanceModel
    {
        /// <summary>
        /// The asset id.
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// The current amount of balance.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Reserved amount of balance
        /// </summary>
        public decimal Reserved { get; set; }

        /// <summary>
        /// Available amount of balance
        /// </summary>
        public decimal Available { get; set; }
    }
}

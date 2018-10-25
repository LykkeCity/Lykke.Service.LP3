namespace Lykke.Service.LP3.Client.Models.Orders
{
    public enum LimitOrderError
    {
        None = 0,

        Unknown = 1,

        LowBalance = 401,

        NoLiquidity = 411,

        NotEnoughFunds = 412,

        ReservedVolumeHigherThanBalance = 414,

        BalanceLowerThanReserved = 416,

        LeadToNegativeSpread = 417,

        TooSmallVolume = 418,

        InvalidPrice = 420
    }
}

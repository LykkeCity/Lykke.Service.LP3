using System.ComponentModel;
using Lykke.MatchingEngine.Connector.Models.Api;
using Lykke.MatchingEngine.Connector.Models.Common;
using Lykke.Service.LP3.Domain.Orders;

namespace Lykke.Service.LP3.DomainServices.Extensions
{
    public static class MatchingEngineMappingExtensions
    {
        public static OrderAction ToOrderAction(this TradeType tradeType)
        {
            if (tradeType == TradeType.Buy)
                return OrderAction.Buy;

            if (tradeType == TradeType.Sell)
                return OrderAction.Sell;

            throw new InvalidEnumArgumentException(nameof(tradeType), (int)tradeType, typeof(TradeType));
        }
        
        public static LimitOrderError ToOrderError(this MeStatusCodes meStatusCode)
        {
            switch (meStatusCode)
            {
                case MeStatusCodes.Ok:
                    return LimitOrderError.None;
                case MeStatusCodes.LowBalance:
                    return LimitOrderError.LowBalance;
                case MeStatusCodes.NoLiquidity:
                    return LimitOrderError.NoLiquidity;
                case MeStatusCodes.NotEnoughFunds:
                    return LimitOrderError.NotEnoughFunds;
                case MeStatusCodes.ReservedVolumeHigherThanBalance:
                    return LimitOrderError.ReservedVolumeHigherThanBalance;
                case MeStatusCodes.BalanceLowerThanReserved:
                    return LimitOrderError.BalanceLowerThanReserved;
                case MeStatusCodes.LeadToNegativeSpread:
                    return LimitOrderError.LeadToNegativeSpread;
                case MeStatusCodes.TooSmallVolume:
                    return LimitOrderError.TooSmallVolume;
                case MeStatusCodes.InvalidPrice:
                    return LimitOrderError.InvalidPrice;
                default:
                    return LimitOrderError.Unknown;
            }
        }
    }
}

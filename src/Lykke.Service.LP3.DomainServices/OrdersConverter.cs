using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Services;
using Lykke.Service.LP3.Domain.Settings;

namespace Lykke.Service.LP3.DomainServices
{
    public class OrdersConverter : IOrdersConverter
    {
        private readonly ICrossRateService _crossRateService;
        private readonly ILog _log;

        public OrdersConverter(
            ILogFactory logFactory,
            ICrossRateService crossRateService)
        {
            _crossRateService = crossRateService;
            _log = logFactory.CreateLog(this);
        }
        
        public async Task<LimitOrder> ConvertAsync(LimitOrder order, AssetPairSettings assetPairSettings) // TODO: add crossTickPrice as a third parameter
        {
            if (order == null) throw new ArgumentNullException(nameof(order));
            if (assetPairSettings == null) throw new ArgumentNullException(nameof(assetPairSettings));
            if (order.TradeType != TradeType.Buy && order.TradeType != TradeType.Sell) 
                throw new ArgumentOutOfRangeException(nameof(order.TradeType), order.TradeType, null);
            
            var crossTickPrice = await _crossRateService
                .GetLastTickPriceAsync(assetPairSettings.CrossInstrumentSource, assetPairSettings.CrossInstrumentAssetPair);

            if (crossTickPrice == null)
            {
                throw new Exception($"No tick price for convert to {assetPairSettings.AssetPairId}");
            }
            
            // there are 4 cases of conversions:
            // 1) LKKCHF -> LKKUSD via CHFUSD (dependent asset pair is not reversed, cross rate is not reversed)
            // 2) LKKCHF -> LKKUSD via USDCHF (dependent asset pair is not reversed, cross rate is reversed)
            // 3) LKKCHF -> BTCLKK via BTCCHF (dependent asset pair is reversed, cross rate is reversed)
            // 4) LKKCHF -> BTCLKK via CHFBTC (dependent asset pair is reversed, cross rate is not reversed)
            // every case have 2 subcases: for sell and for buy


            decimal convertedPrice;
            decimal convertedVolume;
            TradeType convertedTradeType;
            
            
            // convert by cases:
            if (!assetPairSettings.IsReversed && !assetPairSettings.IsCrossInstrumentReversed) // case 1: LKKCHF -> LKKUSD via CHFUSD
            {
                convertedVolume = order.Volume;
                convertedTradeType = order.TradeType;

                if (order.TradeType == TradeType.Sell)
                {
                    convertedPrice = order.Price * crossTickPrice.Ask;
                }
                else
                {
                    convertedPrice = order.Price * crossTickPrice.Bid;
                }
            }
            else if (!assetPairSettings.IsReversed && assetPairSettings.IsCrossInstrumentReversed) // case 2: LKKCHF -> LKKUSD via USDCHF
            {
                convertedVolume = order.Volume;
                convertedTradeType = order.TradeType;

                if (order.TradeType == TradeType.Sell)
                {
                    convertedPrice = order.Price / crossTickPrice.Bid;
                }
                else
                {
                    convertedPrice = order.Price / crossTickPrice.Ask;
                }
            }
            else if (assetPairSettings.IsReversed && assetPairSettings.IsCrossInstrumentReversed) // case 3: LKKCHF -> BTCLKK via BTCCHF
            {
                convertedTradeType = ReverseTradeType(order.TradeType);

                if (order.TradeType == TradeType.Sell)
                {
                    convertedVolume = order.Volume * order.Price / crossTickPrice.Bid;
                    convertedPrice = crossTickPrice.Bid / order.Price;
                }
                else
                {
                    convertedVolume = order.Volume * order.Price / crossTickPrice.Ask;
                    convertedPrice = crossTickPrice.Ask / order.Price;
                }
            }
            else if (assetPairSettings.IsReversed && !assetPairSettings.IsCrossInstrumentReversed) // case 4: LKKCHF -> BTCLKK via CHFBTC
            {
                convertedTradeType = ReverseTradeType(order.TradeType);

                if (order.TradeType == TradeType.Sell)
                {
                    convertedVolume = order.Volume * order.Price * crossTickPrice.Ask;
                    convertedPrice = 1m / (order.Price * crossTickPrice.Ask);
                }
                else
                {
                    convertedVolume = order.Volume * order.Price * crossTickPrice.Bid;
                    convertedPrice = 1m / (order.Price * crossTickPrice.Bid);
                }
            }
            else
            {
                throw new Exception($"Impossible case: {assetPairSettings.ToJson()}");
            }
            
            var convertedOrder = new LimitOrder(convertedPrice, convertedVolume, convertedTradeType);

            _log.Info("Order converted", context: $"from: {order.ToJson()}, to: {convertedOrder.ToJson()}");
            
            return convertedOrder;
        }

        private TradeType ReverseTradeType(TradeType tradeType)
        {
            switch (tradeType)
            {
                case TradeType.Buy:
                    return TradeType.Sell;
                case TradeType.Sell:
                    return TradeType.Buy;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tradeType), tradeType, null);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.LP3.Domain;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Services;
using Lykke.Service.LP3.Domain.Settings;
using Lykke.Service.LP3.DomainServices.Extensions;

namespace Lykke.Service.LP3.DomainServices
{
    public class TradesConverter : ITradesConverter
    {
        private readonly ICrossRateService _crossRateService;
        private readonly ILog _log;

        public TradesConverter(ILogFactory logFactory,
            ICrossRateService crossRateService)
        {
            _crossRateService = crossRateService;
            _log = logFactory.CreateLog(this);
        }
        
        public async Task<IEnumerable<Trade>> ConvertAsync(IReadOnlyList<Trade> trades, AssetPairSettings assetPairSettings)
        {
            var crossTickPrice = await _crossRateService
                .GetLastTickPriceAsync(assetPairSettings.CrossInstrumentSource, assetPairSettings.CrossInstrumentAssetPair);

            if (crossTickPrice == null)
            {
                throw new Exception($"No tick price for convert from {assetPairSettings.AssetPairId}");
            }

            return trades.Select(x => Convert(x, assetPairSettings, crossTickPrice));
        }

        private Trade Convert(Trade trade, AssetPairSettings assetPairSettings, TickPrice crossTickPrice)
        {
            if (trade == null) throw new ArgumentNullException(nameof(trade));
            if (assetPairSettings == null) throw new ArgumentNullException(nameof(assetPairSettings));
            if (crossTickPrice == null) throw new ArgumentNullException(nameof(crossTickPrice));
            if (trade.Type != TradeType.Buy && trade.Type != TradeType.Sell) 
                throw new ArgumentOutOfRangeException(nameof(trade.Type), trade.Type, null);

            decimal convertedVolume;
            decimal convertedPrice;
            TradeType convertedTradeType;
            
            // there are 4 cases of conversions:
            // 1) LKKUSD -> LKKCHF -via CHFUSD (dependent asset pair is not reversed, cross rate is not reversed)
            // 2) LKKUSD -> LKKCHF via USDCHF (dependent asset pair is not reversed, cross rate is reversed)
            // 3) BTCLKK -> LKKCHF via BTCCHF (dependent asset pair is reversed, cross rate is reversed)
            // 4) BTCLKK -> LKKCHF via CHFBTC (dependent asset pair is reversed, cross rate is not reversed)
            // every case have 2 subcases: for sell and for buy

            // convert by cases:
            if (!assetPairSettings.IsReversed && !assetPairSettings.IsCrossInstrumentReversed) // case 1: LKKUSD -> LKKCHF via CHFUSD
            {
                convertedVolume = trade.Volume;
                convertedTradeType = trade.Type;

                if (trade.Type == TradeType.Sell)
                {
                    convertedPrice = trade.Price / crossTickPrice.Ask;
                }
                else
                {
                    convertedPrice = trade.Price / crossTickPrice.Bid;
                }
            }
            else if (!assetPairSettings.IsReversed && assetPairSettings.IsCrossInstrumentReversed) // case 2: LKKCHF -> LKKUSD via USDCHF
            {
                convertedVolume = trade.Volume;
                convertedTradeType = trade.Type;

                if (trade.Type == TradeType.Sell)
                {
                    convertedPrice = trade.Price * crossTickPrice.Bid;
                }
                else
                {
                    convertedPrice = trade.Price * crossTickPrice.Ask;
                }
            }
            else if (assetPairSettings.IsReversed && assetPairSettings.IsCrossInstrumentReversed) // case 3: LKKCHF -> BTCLKK via BTCCHF
            {
                convertedTradeType = trade.Type.Reverse();

                if (trade.Type == TradeType.Sell)
                {
                    convertedVolume = trade.Volume / (trade.Price / crossTickPrice.Ask);
                    convertedPrice = crossTickPrice.Ask / trade.Price;
                }
                else
                {
                    convertedVolume = trade.Volume * (trade.Price / crossTickPrice.Bid);
                    convertedPrice = crossTickPrice.Bid / trade.Price;
                }
            }
            else if (assetPairSettings.IsReversed && !assetPairSettings.IsCrossInstrumentReversed) // case 4: LKKCHF -> BTCLKK via CHFBTC
            {
                convertedTradeType = trade.Type.Reverse();

                if (trade.Type == TradeType.Sell)
                {
                    convertedVolume = trade.Volume / (trade.Price * crossTickPrice.Bid);
                    convertedPrice = 1m / (trade.Price * crossTickPrice.Bid);
                }
                else
                {
                    convertedVolume = trade.Volume / (trade.Price * crossTickPrice.Bid);
                    convertedPrice = 1m / (trade.Price * crossTickPrice.Ask);
                }
            }
            else
            {
                throw new Exception($"Impossible case: {assetPairSettings.ToJson()}");
            }
            
            var convertedTrade = new Trade()
            {
                AssetPairId = assetPairSettings.AssetPairId,
                Price = convertedPrice,
                Type = convertedTradeType,
                Volume = convertedVolume,
                Time = trade.Time
            };

            _log.Info("Trade was converted", context: $"from: {trade.ToJson()}, to: {convertedTrade.ToJson()}");
            
            return convertedTrade;
        }
    }
}

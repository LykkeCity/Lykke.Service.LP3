using System;
using Common;
using Lykke.Service.LP3.Domain;
using Lykke.Service.LP3.Domain.Assets;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Settings;
using Lykke.Service.LP3.DomainServices.Extensions;

namespace Lykke.Service.LP3.DomainServices
{
    public class OrdersConverterLogic
    {
        public DependentLimitOrder Convert(LimitOrder order, AssetPairSettings assetPairSettings, 
            TickPrice crossTickPrice, AssetPairInfo assetPairInfo)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));
            if (assetPairSettings == null) throw new ArgumentNullException(nameof(assetPairSettings));
            if (order.TradeType != TradeType.Buy && order.TradeType != TradeType.Sell) 
                throw new ArgumentOutOfRangeException(nameof(order.TradeType), order.TradeType, null);
            if (crossTickPrice == null) throw new ArgumentNullException(nameof(crossTickPrice));
            if (assetPairInfo == null) throw new ArgumentNullException(nameof(assetPairInfo));
            
            
            // there are 4 cases of conversions:
            // 1) LKKCHF -> LKKUSD via CHFUSD (dependent asset pair is not reversed, cross rate is not reversed)
            // 2) LKKCHF -> LKKUSD via USDCHF (dependent asset pair is not reversed, cross rate is reversed)
            // 3) LKKCHF -> BTCLKK via BTCCHF (dependent asset pair is reversed, cross rate is reversed)
            // 4) LKKCHF -> BTCLKK via CHFBTC (dependent asset pair is reversed, cross rate is not reversed)
            // every case have 2 subcases: for sell and for buy

            decimal convertedPrice;
            decimal convertedVolume;
            TradeType convertedTradeType;
            string conversionDescription;
            
            // convert by cases:
            if (!assetPairSettings.IsReversed && !assetPairSettings.IsCrossInstrumentReversed) // case 1: LKKCHF -> LKKUSD via CHFUSD
            {
                convertedVolume = order.Volume;
                convertedTradeType = order.TradeType;

                if (order.TradeType == TradeType.Sell)
                {
                    convertedPrice = order.Price * crossTickPrice.Ask;
                    conversionDescription = $"New price {convertedPrice} = original price {order.Price} * crossTickPrice.Ask {crossTickPrice.Ask}";
                }
                else
                {
                    convertedPrice = order.Price * crossTickPrice.Bid;
                    conversionDescription = $"New price {convertedPrice} = original price {order.Price} * crossTickPrice.Bid {crossTickPrice.Bid}";
                }
            }
            else if (!assetPairSettings.IsReversed && assetPairSettings.IsCrossInstrumentReversed) // case 2: LKKCHF -> LKKUSD via USDCHF
            {
                convertedVolume = order.Volume;
                convertedTradeType = order.TradeType;

                if (order.TradeType == TradeType.Sell)
                {
                    convertedPrice = order.Price / crossTickPrice.Bid;
                    conversionDescription = $"New price {convertedPrice} = original price {order.Price} / crossTickPrice.Bid {crossTickPrice.Bid}";
                }
                else
                {
                    convertedPrice = order.Price / crossTickPrice.Ask;
                    conversionDescription = $"New price {convertedPrice} = original price {order.Price} / crossTickPrice.Ask {crossTickPrice.Ask}";
                }
            }
            else if (assetPairSettings.IsReversed && assetPairSettings.IsCrossInstrumentReversed) // case 3: LKKCHF -> BTCLKK via BTCCHF
            {
                convertedTradeType = order.TradeType.Reverse();

                if (order.TradeType == TradeType.Sell)
                {
                    convertedVolume = order.Volume * order.Price / crossTickPrice.Bid;
                    convertedPrice = crossTickPrice.Bid / order.Price;
                    
                    conversionDescription = $"New price {convertedPrice} = crossTickPrice.Bid {crossTickPrice.Bid} / original price {order.Price}, " +
                                            $"New volume {convertedVolume} = original volume {order.Volume} / converted price {convertedPrice}";
                }
                else
                {
                    convertedVolume = order.Volume * order.Price / crossTickPrice.Ask;
                    convertedPrice = crossTickPrice.Ask / order.Price;
                    
                    conversionDescription = $"New price {convertedPrice} = crossTickPrice.Ask {crossTickPrice.Ask} / original price {order.Price}, " +
                                            $"New volume {convertedVolume} = original volume {order.Volume} / converted price {convertedPrice}";
                }
            }
            else if (assetPairSettings.IsReversed && !assetPairSettings.IsCrossInstrumentReversed) // case 4: LKKCHF -> BTCLKK via CHFBTC
            {
                convertedTradeType = order.TradeType.Reverse();

                if (order.TradeType == TradeType.Sell)
                {
                    convertedVolume = order.Volume * order.Price * crossTickPrice.Ask;
                    convertedPrice = 1m / (order.Price * crossTickPrice.Ask);
                    
                    conversionDescription = $"New price {convertedPrice} = 1 / (original price {order.Price}) * crossTickPrice.Ask {crossTickPrice.Ask}), " +
                                            $"New volume {convertedVolume} = original volume {order.Volume} * converted price {convertedPrice}";
                }
                else
                {
                    convertedVolume = order.Volume * order.Price * crossTickPrice.Bid;
                    convertedPrice = 1m / (order.Price * crossTickPrice.Bid);
                    
                    conversionDescription = $"New price {convertedPrice} = 1 / (original price {order.Price}) * crossTickPrice.Bid {crossTickPrice.Bid}), " +
                                            $"New volume {convertedVolume} = original volume {order.Volume} * converted price {convertedPrice}";
                }
            }
            else
            {
                throw new Exception($"Impossible case: {assetPairSettings.ToJson()}");
            }
            
            
            var convertedOrder = new DependentLimitOrder(order.Id, convertedPrice, convertedVolume, convertedTradeType)
            {
                AssetPairId = assetPairSettings.AssetPairId,
                BaseLimitOrder = order,
                CrossTickPrice = crossTickPrice,
                Description = conversionDescription
            };
            
            convertedOrder.Round(assetPairInfo);

            return convertedOrder;
        }
    }
}
using Lykke.Service.LP3.Domain;
using Lykke.Service.LP3.Domain.Assets;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Settings;
using Lykke.Service.LP3.DomainServices;
using Xunit;

namespace Lykke.Service.LP3.Tests
{
    public class OrderConverterTests
    {
        [Fact]
        public void Test()
        {
            var baseAssetPair = "LKKCHF";

            var dependentPairSettings = new AssetPairSettings
            {
                AssetPairId = "LKKUSD",
                IsReversed = false,
                CrossInstrumentSource = "externalExchange",
                CrossInstrumentAssetPair = "CHFUSD",
                IsCrossInstrumentReversed = false
            };

            var crossTickPrice = new TickPrice
            {
                Source = dependentPairSettings.CrossInstrumentSource,
                AssetPair = dependentPairSettings.CrossInstrumentAssetPair,
                Ask = 2.1m,
                Bid = 1.9m
            };
            
            var assetPairInfo = new AssetPairInfo
            {
                AssetPairId = baseAssetPair,
                MinVolume = 0m,
                PriceAccuracy = 2,
                VolumeAccuracy = 2
            };
            
            var converter = new OrdersConverterLogic();

            var baseOrder = new LimitOrder(price: 100m, volume: 10m, tradeType: TradeType.Sell);


            var convertedOrder = converter.Convert(baseOrder, dependentPairSettings, crossTickPrice, assetPairInfo);
            
            Assert.NotNull(convertedOrder);
            Assert.Equal(baseOrder.Volume, convertedOrder.Volume);
            Assert.Equal(baseOrder.TradeType, convertedOrder.TradeType);
            Assert.Equal(100m * 2.1m, convertedOrder.Price);
        }
    }
}

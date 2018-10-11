using System.Threading.Tasks;
using Lykke.Logs;
using Lykke.Service.LP3.Domain;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Services;
using Lykke.Service.LP3.Domain.Settings;
using Lykke.Service.LP3.DomainServices;
using Moq;
using Xunit;

namespace Lykke.Service.LP3.Tests
{
    public class OrderConverterTests
    {
        [Fact]
        public async Task Test()
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
            
            var crossRateServiceMock = new Mock<ICrossRateService>();
            crossRateServiceMock.Setup(x => x.GetLastTickPriceAsync(dependentPairSettings.CrossInstrumentSource,
                    dependentPairSettings.CrossInstrumentAssetPair))
                .ReturnsAsync(crossTickPrice);
            
            var converter = new OrdersConverter(EmptyLogFactory.Instance, crossRateServiceMock.Object);

            var baseOrder = new LimitOrder(price: 100m, volume: 10m, tradeType: TradeType.Sell);


            var convertedOrder = await converter.ConvertAsync(baseOrder, dependentPairSettings);
            
            Assert.NotNull(convertedOrder);
            Assert.Equal(baseOrder.Volume, convertedOrder.Volume);
            Assert.Equal(baseOrder.TradeType, convertedOrder.TradeType);
            Assert.Equal(100m * 2.1m, convertedOrder.Price);
        }
    }
}

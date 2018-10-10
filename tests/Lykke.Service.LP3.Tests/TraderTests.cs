using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Logs;
using Lykke.Service.Assets.Client.Models.v3;
using Lykke.Service.Assets.Client.ReadModels;
using Lykke.Service.LP3.Domain;
using Lykke.Service.LP3.Domain.Exchanges;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Repositories;
using Lykke.Service.LP3.Domain.Services;
using Lykke.Service.LP3.Domain.Settings;
using Lykke.Service.LP3.DomainServices;
using Moq;
using Xunit;

namespace Lykke.Service.LP3.Tests
{
    public class TraderTests
    {
        private Lp3Service CreateTrader()
        {
            var levels = new[]
            {
                new Level("1", 0.001m, 10),
                new Level("2", 0.002m, 10),
                new Level("3", 0.004m, 10),
                new Level("4", 0.008m, 10)
            };

            var levelRepositoryMock = new Mock<ILevelRepository>();
            levelRepositoryMock.Setup(x => x.GetLevels()).ReturnsAsync(levels);
            
            var levelsService = new LevelsService(EmptyLogFactory.Instance, levelRepositoryMock.Object);
            levelsService.Start();

            var initialPriceServiceMock = new Mock<IInitialPriceService>();
            initialPriceServiceMock.Setup(x => x.GetAsync())
                .ReturnsAsync(new InitialPrice(1000m));

            var assetsServiceMock = new Mock<IAssetPairsReadModelRepository>();
            assetsServiceMock.Setup(x => x.TryGet(It.IsAny<string>()))
                .Returns(new AssetPair());

            var settingsServiceMock = new Mock<ISettingsService>();
            settingsServiceMock.Setup(x => x.GetBaseAssetPairSettings())
                .ReturnsAsync(new BaseAssetPairSettings{ AssetPairId = "LKKCHF" });

            var additionalVolumeServiceMock = new Mock<IAdditionalVolumeService>();
            additionalVolumeServiceMock.Setup(x => x.GetOrdersAsync(It.IsAny<IEnumerable<LimitOrder>>()))
                .ReturnsAsync(Enumerable.Empty<LimitOrder>());
            
            var trader = new Lp3Service(EmptyLogFactory.Instance, 
                settingsServiceMock.Object,
                levelsService,
                additionalVolumeServiceMock.Object,
                initialPriceServiceMock.Object,
                Mock.Of<ILykkeExchange>(),
                assetsServiceMock.Object);
            
            trader.Start();

            return trader;
        }
        
        [Fact]
        public void GetOrdersTest()
        {
            var trader = CreateTrader();
            
            var orders = trader.GetOrders().ToList();
            var sellOrders = orders.Where(x => x.TradeType == TradeType.Sell).OrderBy(x => x.Price).ToList();
            var buyOrders = orders.Where(x => x.TradeType == TradeType.Buy).OrderByDescending(x => x.Price).ToList();
            
            Assert.Equal(8, orders.Count);
            Assert.True(orders.All(x => x.Volume == 10m));
            
            Assert.Equal(1001m, sellOrders[0].Price, precision: 0);
            Assert.Equal(1002m, sellOrders[1].Price, precision: 0);
            Assert.Equal(1004m, sellOrders[2].Price, precision: 0);
            Assert.Equal(1008m, sellOrders[3].Price, precision: 0);
            
            Assert.Equal(999m, buyOrders[0].Price, precision: 0);
            Assert.Equal(998m, buyOrders[1].Price, precision: 0);
            Assert.Equal(996m, buyOrders[2].Price, precision: 0);
            Assert.Equal(992m, buyOrders[3].Price, precision: 0);
        }

        [Fact]
        public async Task HandleBuyExecution_SingleVolume()
        {
            var trader = CreateTrader();

            await trader.HandleTradesAsync(new [] { new Trade
            {
                Type = TradeType.Buy,
                Volume = 10
            }});
            
            var orders = trader.GetOrders().ToList();
            var sellOrders = orders.Where(x => x.TradeType == TradeType.Sell).OrderBy(x => x.Price).ToList();
            var buyOrders = orders.Where(x => x.TradeType == TradeType.Buy).OrderByDescending(x => x.Price).ToList();
            
            Assert.Equal(8, orders.Count);
            Assert.True(orders.All(x => x.Volume == 10m));
            
            Assert.Equal(1000m, sellOrders[0].Price, precision: 0); // the price goes down
            Assert.Equal(1002m, sellOrders[1].Price, precision: 0);
            Assert.Equal(1004m, sellOrders[2].Price, precision: 0);
            Assert.Equal(1008m, sellOrders[3].Price, precision: 0);
            
            Assert.Equal(998m, buyOrders[0].Price, precision: 0); // the price goes up
            Assert.Equal(998m, buyOrders[1].Price, precision: 0);
            Assert.Equal(996m, buyOrders[2].Price, precision: 0);
            Assert.Equal(992m, buyOrders[3].Price, precision: 0);
        }

        [Fact]
        public async Task HandleBuyExecution_DoubleVolume()
        {
            var trader = CreateTrader();

            await trader.HandleTradesAsync(new [] { new Trade
            {
                Type = TradeType.Buy,
                Volume = 20
            }});
            
            var orders = trader.GetOrders().ToList();
            var sellOrders = orders.Where(x => x.TradeType == TradeType.Sell).OrderBy(x => x.Price).ToList();
            var buyOrders = orders.Where(x => x.TradeType == TradeType.Buy).OrderByDescending(x => x.Price).ToList();
            
            Assert.Equal(8, orders.Count);
            Assert.True(orders.All(x => x.Volume == 10m));
            
            Assert.Equal(1000m, sellOrders[0].Price, precision: 0);
            Assert.Equal(1000m, sellOrders[1].Price, precision: 0);
            Assert.Equal(1004m, sellOrders[2].Price, precision: 0);
            Assert.Equal(1008m, sellOrders[3].Price, precision: 0);
            
            Assert.Equal(998m, buyOrders[0].Price, precision: 0);
            Assert.Equal(996m, buyOrders[1].Price, precision: 0);
            Assert.Equal(996m, buyOrders[2].Price, precision: 0);
            Assert.Equal(992m, buyOrders[3].Price, precision: 0);
        }

        [Fact]
        public async Task HandleBuyExecution_TripleVolume()
        {
            var trader = CreateTrader();

            await trader.HandleTradesAsync(new [] { new Trade
            {
                Type = TradeType.Buy,
                Volume = 30
            }});
            
            var orders = trader.GetOrders().ToList();
            var sellOrders = orders.Where(x => x.TradeType == TradeType.Sell).OrderBy(x => x.Price).ToList();
            var buyOrders = orders.Where(x => x.TradeType == TradeType.Buy).OrderByDescending(x => x.Price).ToList();
            
            Assert.Equal(8, orders.Count);
            Assert.True(orders.All(x => x.Volume == 10m));
            
            Assert.Equal(999m, sellOrders[0].Price, precision: 0);
            Assert.Equal(1000m, sellOrders[1].Price, precision: 0);
            Assert.Equal(1004m, sellOrders[2].Price, precision: 0);
            Assert.Equal(1008m, sellOrders[3].Price, precision: 0);
            
            Assert.Equal(997m, buyOrders[0].Price, precision: 0);
            Assert.Equal(996m, buyOrders[1].Price, precision: 0);
            Assert.Equal(996m, buyOrders[2].Price, precision: 0);
            Assert.Equal(992m, buyOrders[3].Price, precision: 0);
        }
    }
}

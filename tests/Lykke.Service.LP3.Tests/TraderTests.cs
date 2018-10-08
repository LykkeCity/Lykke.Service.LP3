using System.Linq;
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
    public class TraderTests
    {
        private TradingAlgorithm CreateTrader()
        {
            var levelsServiceMock = new Mock<ILevelsService>();
            levelsServiceMock.Setup(x => x.GetLevels())
                .ReturnsAsync(new[]
                {
                    new Level(new LevelSettings("1", 10, 0.001m)),
                    new Level(new LevelSettings("2", 10, 0.002m)),
                    new Level(new LevelSettings("3", 10, 0.004m)),
                    new Level(new LevelSettings("4", 10, 0.008m))
                });
            
            return new TradingAlgorithm(EmptyLogFactory.Instance, levelsServiceMock.Object);
        }
        
        [Fact]
        public async Task GetOrdersTest()
        {
            var trader = CreateTrader();

            await trader.StartAsync(startMid: 1000m);
            
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

            await trader.StartAsync(startMid: 1000m);

            trader.HandleTrade(new Trade
            {
                Type = TradeType.Buy,
                Volume = 10
            });
            
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

            await trader.StartAsync(startMid: 1000m);

            trader.HandleTrade(new Trade
            {
                Type = TradeType.Buy,
                Volume = 20
            });
            
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

            await trader.StartAsync(startMid: 1000m);

            trader.HandleTrade(new Trade
            {
                Type = TradeType.Buy,
                Volume = 30
            });
            
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

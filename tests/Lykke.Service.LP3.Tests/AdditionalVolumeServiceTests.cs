using System.Linq;
using System.Threading.Tasks;
using Lykke.Logs;
using Lykke.Service.LP3.Domain.Assets;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.TradingAlgorithm;
using Xunit;

namespace Lykke.Service.LP3.Tests
{
    public class AdditionalVolumeServiceTests
    {
        private AssetPairInfo AssetPairInfo => new AssetPairInfo
        {
            AssetPairId = "LKKCHF",
            MinVolume = 0m,
            PriceAccuracy = 2,
            VolumeAccuracy = 2
        };

        private const int Count = 3;
        private const decimal Delta = 0.01m;
        private const decimal Volume = 10;
        
        [Fact]
        public void FullOrderBook_UseWorstAskAndBid()
        {
            var currentOrders = new[]
            {
                new LimitOrder(101, 10, TradeType.Buy),
                new LimitOrder(102, 10, TradeType.Buy),
                new LimitOrder(103, 10, TradeType.Sell),
                new LimitOrder(104, 10, TradeType.Sell)
            };
            
            var service = new AdditionalOrdersGenerator();

            var orders = service.GetOrders(currentOrders, Count, Volume, Delta).ToList();
            var sellOrders = orders.Where(x => x.TradeType == TradeType.Sell).OrderBy(x => x.Price).ToList();
            var buyOrders = orders.Where(x => x.TradeType == TradeType.Buy).OrderByDescending(x => x.Price).ToList();
            
            Assert.Equal(Count * 2, orders.Count);
            Assert.True(orders.All(x => x.Volume == Volume));
            
            Assert.Equal(105m, sellOrders[0].Price, precision: 0);
            Assert.Equal(106m, sellOrders[1].Price, precision: 0);
            Assert.Equal(107m, sellOrders[2].Price, precision: 0);
            
            Assert.Equal(100m, buyOrders[0].Price, precision: 0);
            Assert.Equal(99m, buyOrders[1].Price, precision: 0);
            Assert.Equal(98m, buyOrders[2].Price, precision: 0);
        }
        
        [Fact]
        public async Task OnlyAsks_UseWorstAndBestAsk()
        {
            var currentOrders = new[]
            {
                new LimitOrder(103, 10, TradeType.Sell),
                new LimitOrder(104, 10, TradeType.Sell)
            };
            
            var service = new AdditionalOrdersGenerator();

            var orders = service.GetOrders(currentOrders, Count, Volume, Delta).ToList();
            var sellOrders = orders.Where(x => x.TradeType == TradeType.Sell).OrderBy(x => x.Price).ToList();
            var buyOrders = orders.Where(x => x.TradeType == TradeType.Buy).OrderByDescending(x => x.Price).ToList();
            
            Assert.Equal(Count * 2, orders.Count);
            Assert.True(orders.All(x => x.Volume == Volume));
            
            Assert.Equal(105m, sellOrders[0].Price, precision: 0);
            Assert.Equal(106m, sellOrders[1].Price, precision: 0);
            Assert.Equal(107m, sellOrders[2].Price, precision: 0);
            
            Assert.Equal(102m, buyOrders[0].Price, precision: 0);
            Assert.Equal(101m, buyOrders[1].Price, precision: 0);
            Assert.Equal(100m, buyOrders[2].Price, precision: 0);
        }
        
        [Fact]
        public async Task OnlyBids_UseBestAndWorstBid()
        {
            var currentOrders = new[]
            {
                new LimitOrder(101, 10, TradeType.Buy),
                new LimitOrder(102, 10, TradeType.Buy),
            };
            
            var service = new AdditionalOrdersGenerator();

            var orders = service.GetOrders(currentOrders, Count, Volume, Delta).ToList();
            var sellOrders = orders.Where(x => x.TradeType == TradeType.Sell).OrderBy(x => x.Price).ToList();
            var buyOrders = orders.Where(x => x.TradeType == TradeType.Buy).OrderByDescending(x => x.Price).ToList();
            
            Assert.Equal(Count * 2, orders.Count);
            Assert.True(orders.All(x => x.Volume == Volume));
            
            Assert.Equal(103m, sellOrders[0].Price, precision: 0);
            Assert.Equal(104m, sellOrders[1].Price, precision: 0);
            Assert.Equal(105m, sellOrders[2].Price, precision: 0);
            
            Assert.Equal(100m, buyOrders[0].Price, precision: 0);
            Assert.Equal(99m, buyOrders[1].Price, precision: 0);
            Assert.Equal(98m, buyOrders[2].Price, precision: 0);
        }
    }
}

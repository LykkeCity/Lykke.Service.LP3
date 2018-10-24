using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Settings;
using Lykke.Service.LP3.Domain.TradingAlgorithm;
using Xunit;

namespace Lykke.Service.LP3.Tests
{
    public class TraderTests
    {
        [Fact]
        public void HandlePartialTrade_RemainingVolumeMoreThenMinimal_NothingAddedNothingRemoved()
        {
            var settings = new OrderBookTraderSettings
            {
                AssetPairId = "BTCAUD",
                Count = 5,
                Delta = 0.1m,
                InitialPrice = 5,
                IsEnabled = true,
                Volume = 10
            };

            var minVolume = 1m;
                
            var trader = new OrderBookTrader(settings);

            var orders = trader.CreateOrders();

            var (addedOrders, removedOrders) = trader.HandleTrades(new[]
            {
                new Trade
                {
                    AssetPairId = "BTCAUD",
                    Type = TradeType.Sell,
                    Price = 5.2m,
                    Volume = 8
                }
            }, minVolume);
            
            Assert.Empty(removedOrders);
            Assert.Empty(addedOrders);
        }

        [Fact]
        public void HandlePartialTrade_RemainingVolumeLessThenMinimal_OneAddedOneRemoved()
        {
            var settings = new OrderBookTraderSettings
            {
                AssetPairId = "BTCAUD",
                Count = 5,
                Delta = 0.1m,
                InitialPrice = 5,
                IsEnabled = true,
                Volume = 10
            };

            var minVolume = 3m;
                
            var trader = new OrderBookTrader(settings);

            var orders = trader.CreateOrders();

            var (addedOrders, removedOrders) = trader.HandleTrades(new[]
            {
                new Trade
                {
                    AssetPairId = "BTCAUD",
                    Type = TradeType.Sell,
                    Price = 5.2m,
                    Volume = 8
                }
            }, minVolume);
            
            Assert.Equal(1, removedOrders.Count);
            Assert.Equal(1, addedOrders.Count);
        }
        
    }
}

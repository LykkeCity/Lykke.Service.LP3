using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Common.Log;
using Lykke.Common.Log;
using Lykke.MatchingEngine.ExchangeModels;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Settings;
using Lykke.Service.LP3.Domain.TradingAlgorithm;
using Moq;
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

            var trader = new OrderBookTrader(settings, FakeLogFactory());

            var orders = trader.CreateOrders();

            var (addedOrders, removedOrders, toCancelOrders) = trader.HandleTrades(new[]
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

            var trader = new OrderBookTrader(settings, FakeLogFactory());

            var orders = trader.CreateOrders();

            var (addedOrders, removedOrders, toCancelOrders) = trader.HandleTrades(new[]
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

        [Fact]
        public void OrdersCreation_HandleInconsistentSettings()
        {
            var settings = new OrderBookTraderSettings
            {
                AssetPairId = "BTCAUD",
                Count = 6,
                Delta = 0.1m,
                InitialPrice = 5,
                IsEnabled = true,
                Volume = 10,
                MinCountOrderInMarket = 5,
                AddedCountOrdersInMarket = 8
            };

            Assert.Throws<ArgumentException>(() => new OrderBookTrader(settings, FakeLogFactory()));
        }

        [Theory]
        [InlineData(5, 8)]
        public void OrdersCreation_HandleInit(int minCountOrderInMarket, int addedCountOrdersInMarket)
        {
            var settings = new OrderBookTraderSettings
            {
                AssetPairId = "BTCAUD",
                Count = 100,
                Delta = 0.1m,
                InitialPrice = 500,
                IsEnabled = true,
                Volume = 10,
                MinCountOrderInMarket = minCountOrderInMarket,
                AddedCountOrdersInMarket = addedCountOrdersInMarket
            };

            var trader = new OrderBookTrader(settings, FakeLogFactory());

            var orders = trader.CreateOrders();

            Assert.Equal(orders.Count, (minCountOrderInMarket + addedCountOrdersInMarket) * 2);
        }

        [Theory]
        [InlineData(TradeType.Sell, 10, 6, 2, 1, 0, 1, 1, 0, 0)]
        [InlineData(TradeType.Sell, 10, 6, 2, 2, 2, 2, 2, 0, 2)]
        [InlineData(TradeType.Sell, 10, 6, 2, 3, 2, 3, 3, 0, 2)]
        [InlineData(TradeType.Sell, 10, 6, 2, 4, 2, 4, 4, 0, 2)]
        public void OrdersCreation_HandleMiddle(
            TradeType tradeType,
            int count,
            int minCountOrderInMarket,
            int addedCountOrdersInMarket,
            int numOfOrdersMatched,
            int sellOrdersCreated,
            int sellOrdersRemoved,
            int buyOrdersCreated,
            int buyOrdersRemoved,
            int toCancelOrdersCount)
        {
            var initialPrice = 100m;
            var delta = 1m;
            var volume = 10m;
            
            var settings = new OrderBookTraderSettings
            {
                AssetPairId = "BTCAUD",
                Count = count,
                Delta = delta,
                InitialPrice = initialPrice,
                IsEnabled = true,
                Volume = volume,
                MinCountOrderInMarket = minCountOrderInMarket,
                AddedCountOrdersInMarket = addedCountOrdersInMarket
            };

            var trader = new OrderBookTrader(settings, FakeLogFactory());

            var orders = trader.CreateOrders();

            var trades = GenerateTrades(tradeType, volume, numOfOrdersMatched);

            var (addedOrders, removedOrders, toCancelOrders) = trader.HandleTrades(trades, 0);
            
            Assert.Equal(sellOrdersCreated, addedOrders.Count(x => x.TradeType == TradeType.Sell));
            Assert.Equal(sellOrdersRemoved, removedOrders.Count(x => x.TradeType == TradeType.Sell));
            
            Assert.Equal(buyOrdersCreated, addedOrders.Count(x => x.TradeType == TradeType.Buy));
            Assert.Equal(buyOrdersRemoved, removedOrders.Count(x => x.TradeType == TradeType.Buy));
            
            Assert.Equal(toCancelOrdersCount, toCancelOrders.Count);
        }

        private IReadOnlyList<Trade> GenerateTrades(TradeType type, decimal volume, int numOfOrders)
        {
            var result = new List<Trade>();

            for (int i = 0; i < numOfOrders; i++)
            {
                result.Add(new Trade
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = type,
                    Volume = volume
                });
            }

            result.Add(new Trade
            {
                Id = Guid.NewGuid().ToString(),
                Type = type,
                Volume = volume * new decimal(new Random().NextDouble())
            });

            return result;
        }

        private ILogFactory FakeLogFactory()
        {
            var mock = new Mock<ILogFactory>();

            mock.Setup(x => x.CreateLog(It.IsAny<object>())).Returns(new LogToConsole());
            
            return mock.Object;
        }
    }
}

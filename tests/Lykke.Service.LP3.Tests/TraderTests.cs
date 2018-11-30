using System;
using System.Collections.Generic;
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

        [Fact]
        public void InventoryUpdate()
        {
            var settings = new OrderBookTraderSettings
            {
                AssetPairId = "BTCJPY",
                Count = 10,
                Delta = 1000m,
                InitialPrice = 100000,
                IsEnabled = true,
                Volume = 0.1m,
                CountInMarket = 8
            };

            var minVolume = 0m;
                
            var trader = new OrderBookTrader(settings);

            var orders = trader.CreateOrders();

            var trades = new List<Trade>()
            {
                new Trade
                {
                    Type = TradeType.Sell,
                    Volume = 0.1m,
                    Price = 103000m
                },
                new Trade
                {
                    Type = TradeType.Sell,
                    Volume = 0.1m,
                    Price = 102000m
                },
                new Trade
                {
                    Type = TradeType.Sell,
                    Volume = 0.1m,
                    Price = 101000m
                },
                new Trade
                {
                    Type = TradeType.Buy,
                    Volume = 0.1m,
                    Price = 99000m
                },
                new Trade
                {
                    Type = TradeType.Buy,
                    Volume = 0.1m,
                    Price = 98000m
                },
                new Trade
                {
                    Type = TradeType.Buy,
                    Volume = 0.1m,
                    Price = 97000m
                },
                new Trade
                {
                    Type = TradeType.Buy,
                    Volume = 0.1m,
                    Price = 97000m
                },
                new Trade
                {
                    Type = TradeType.Buy,
                    Volume = 0.1m,
                    Price = 97000m
                },
                new Trade
                {
                    Type = TradeType.Buy,
                    Volume = 0.1m,
                    Price = 97000m
                },
                new Trade
                {
                    Type = TradeType.Sell,
                    Volume = 0.1m,
                    Price = 98000m
                },
                new Trade
                {
                    Type = TradeType.Sell,
                    Volume = 0.1m,
                    Price = 99000m
                },
                new Trade
                {
                    Type = TradeType.Sell,
                    Volume = 0.1m,
                    Price = 100000m
                }
            };

            trader.HandleTrades(trades, minVolume);
            
            Assert.Equal(trader.Inventory, 0m);
            Assert.Equal(trader.OppositeInventory, 1800m);
        }
    }
}

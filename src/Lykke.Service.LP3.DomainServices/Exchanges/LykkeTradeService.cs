using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.MatchingEngine.Connector.Models.Events;
using Lykke.MatchingEngine.ExchangeModels;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Services;
using LimitOrder = Lykke.MatchingEngine.ExchangeModels.LimitOrder;
using Trade = Lykke.Service.LP3.Domain.Orders.Trade;

namespace Lykke.Service.LP3.DomainServices.Exchanges
{
    [UsedImplicitly]
    public class LykkeTradeService : ILykkeTradeService
    {
        private readonly ISettingsService _settingsService;
        private readonly ITrader _trader;
        private readonly ILogFactory _logFactory;
        private readonly ILog _log;

        public LykkeTradeService(
            ISettingsService settingsService,
            ITrader trader,
            ILogFactory logFactory)
        {
            _settingsService = settingsService;
            _trader = trader;
            _logFactory = logFactory;
            _log = logFactory.CreateLog(this);
        }

        public async Task HandleAsync(LimitOrders limitOrders)
        {
            var sw = new Stopwatch();

            try
            {
                if (limitOrders.Orders == null || limitOrders.Orders.Count == 0)
                    return;

                string walletId = await _settingsService.GetWalletIdAsync();

                if (string.IsNullOrEmpty(walletId))
                    return;

                IEnumerable<LimitOrderWithTrades> clientLimitOrders = limitOrders.Orders
                    .Where(o => o.Order?.ClientId == walletId)
                    .Where(o => o.Trades?.Count > 0);

                IReadOnlyList<Trade> trades = CreateReports(clientLimitOrders);

                foreach (Trade trade in trades)
                {
                    await _trader.HandleTradeAsync(trade);
                }
            }
            catch (Exception exception)
            {
                _log.Error(exception, "An error occurred during processing trades", limitOrders);
            }
            finally
            {
                sw.Stop();
            }
        }

        private static IReadOnlyList<Trade> CreateReports(IEnumerable<LimitOrderWithTrades> limitOrders)
        {
            var executionReports = new List<Trade>();

            foreach (LimitOrderWithTrades limitOrderModel in limitOrders)
            {
                // The limit order fully executed. The remaining volume is zero.
                if (limitOrderModel.Order.Status == OrderStatus.Matched.ToString())
                {
                    IReadOnlyList<Trade> orderExecutionReports =
                        CreateFillReports(limitOrderModel.Order, limitOrderModel.Trades, true);

                    executionReports.AddRange(orderExecutionReports);
                }

                // The limit order partially executed.
                if (limitOrderModel.Order.Status == "Processing")
                {
                    IReadOnlyList<Trade> orderExecutionReports =
                        CreateFillReports(limitOrderModel.Order, limitOrderModel.Trades, false);

                    executionReports.AddRange(orderExecutionReports);
                }

                // The limit order was cancelled by matching engine after processing trades.
                // In this case order partially executed and remaining volume is less than min volume allowed by asset pair.
                if (limitOrderModel.Order.Status == OrderStatus.Cancelled.ToString())
                {
                    IReadOnlyList<Trade> orderExecutionReports =
                        CreateFillReports(limitOrderModel.Order, limitOrderModel.Trades, true);

                    executionReports.AddRange(orderExecutionReports);
                }
            }

            return executionReports;
        }

        private static IReadOnlyList<Trade> CreateFillReports(LimitOrder limitOrder,
            IReadOnlyList<LimitTradeInfo> trades, bool completed)
        {
            var reports = new List<Trade>();

            for (int i = 0; i < trades.Count; i++)
            {
                LimitTradeInfo trade = trades[i];

                TradeType tradeType = limitOrder.Volume < 0
                    ? TradeType.Sell
                    : TradeType.Buy;

                TradeStatus executionStatus = i == trades.Count - 1 && completed
                    ? TradeStatus.Fill
                    : TradeStatus.PartialFill;

                var report = new Trade
                {
                    Id = Guid.NewGuid().ToString(),
                    AssetPairId = limitOrder.AssetPairId,
                    ExchangeOrderId = limitOrder.Id,
                    LimitOrderId = limitOrder.ExternalId,
                    Status = executionStatus,
                    Type = tradeType,
                    Time = trade.Timestamp,
                    Price = (decimal) trade.Price,
                    Volume = tradeType == TradeType.Buy
                        ? (decimal) trade.OppositeVolume
                        : (decimal) trade.Volume,
                    ClientId = trade.ClientId,
                    OppositeClientId = trade.OppositeClientId,
                    OppositeLimitOrderId = trade.OppositeOrderId,
                    OppositeSideVolume = tradeType == TradeType.Buy
                        ? (decimal) trade.Volume
                        : (decimal) trade.OppositeVolume,
                    RemainingVolume = (decimal) limitOrder.RemainingVolume
                };

                reports.Add(report);
            }

            return reports;
        }
    }
}

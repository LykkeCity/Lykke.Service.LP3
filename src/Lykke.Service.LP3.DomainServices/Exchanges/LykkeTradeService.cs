using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.MatchingEngine.Connector.Models.Events;
using Lykke.MatchingEngine.ExchangeModels;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Repositories;
using Lykke.Service.LP3.Domain.Services;
using LimitOrder = Lykke.MatchingEngine.ExchangeModels.LimitOrder;
using Trade = Lykke.Service.LP3.Domain.Orders.Trade;

namespace Lykke.Service.LP3.DomainServices.Exchanges
{
    [UsedImplicitly]
    public class LykkeTradeService : ILykkeTradeService
    {
        private readonly ISettingsService _settingsService;
        private readonly ITradeRepository _tradeRepository;
        private readonly ILp3Service _lp3Service;
        private readonly ILog _log;

        public LykkeTradeService(
            ISettingsService settingsService,
            ITradeRepository tradeRepository,
            ILp3Service lp3Service,
            ILogFactory logFactory)
        {
            _settingsService = settingsService;
            _tradeRepository = tradeRepository;
            _lp3Service = lp3Service;
            _log = logFactory.CreateLog(this);
        }

        public Task<IReadOnlyList<Trade>> GetAsync(DateTime startDate, DateTime endDate)
        {
            return _tradeRepository.GetAsync(startDate, endDate);
        }
        
        public async Task HandleAsync(LimitOrders limitOrders)
        {
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
                
                _log.Info("LimitOrders received", context: $"{limitOrders.ToJson()}");

                IReadOnlyList<Trade> trades = ExtractTrades(clientLimitOrders);

                if (trades.Any())
                {
                    await SaveTrades(trades);
                    await _lp3Service.HandleTradesAsync(trades);
                }
            }
            catch (Exception exception)
            {
                _log.Error(exception, "An error occurred during processing trades", limitOrders);
            }
        }

        private async Task SaveTrades(IReadOnlyList<Trade> trades)
        {
            foreach (var trade in trades)
            {
                try
                {
                    await _tradeRepository.InsertAsync(trade);
                }
                catch (Exception e)
                {
                    _log.Error(e, $"Error on saving trade {trade.ToJson()}");
                }
            }
        }

        private static IReadOnlyList<Trade> ExtractTrades(IEnumerable<LimitOrderWithTrades> limitOrders)
        {
            var resultListOfTrades = new List<Trade>();

            foreach (LimitOrderWithTrades limitOrderModel in limitOrders)
            {
                // The limit order fully executed. The remaining volume is zero.
                if (limitOrderModel.Order.Status == OrderStatus.Matched.ToString())
                {
                    IReadOnlyList<Trade> tradesFromOrder =
                        CreateTradesFromOrder(limitOrderModel.Order, limitOrderModel.Trades, true);

                    resultListOfTrades.AddRange(tradesFromOrder);
                }

                // The limit order partially executed.
                if (limitOrderModel.Order.Status == "Processing")
                {
                    IReadOnlyList<Trade> tradesFromOrder =
                        CreateTradesFromOrder(limitOrderModel.Order, limitOrderModel.Trades, false);

                    resultListOfTrades.AddRange(tradesFromOrder);
                }

                // The limit order was cancelled by matching engine after processing trades.
                // In this case order partially executed and remaining volume is less than min volume allowed by asset pair.
                if (limitOrderModel.Order.Status == OrderStatus.Cancelled.ToString())
                {
                    IReadOnlyList<Trade> tradesFromOrder =
                        CreateTradesFromOrder(limitOrderModel.Order, limitOrderModel.Trades, true);

                    resultListOfTrades.AddRange(tradesFromOrder);
                }
            }

            return resultListOfTrades;
        }

        private static IReadOnlyList<Trade> CreateTradesFromOrder(LimitOrder limitOrder,
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

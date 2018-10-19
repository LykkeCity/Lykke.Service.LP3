using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.MatchingEngine.Connector.Models.Events;
using Lykke.MatchingEngine.ExchangeModels;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Services;
using Lykke.Service.LP3.Settings;
using LimitOrder = Lykke.MatchingEngine.ExchangeModels.LimitOrder;
using Trade = Lykke.Service.LP3.Domain.Orders.Trade;

namespace Lykke.Service.LP3.RabbitMq.Subscribers
{
    public class LykkeLimitOrdersSubscriber : IStartable, IStopable
    {
        private readonly ILogFactory _logFactory;
        private readonly ILykkeTradeService _lykkeTradeService;
        private readonly ISettingsService _settingsService;
        private readonly RabbitMqSettings _settings;
        private RabbitMqSubscriber<LimitOrders> _subscriber;
        private readonly ILog _log;

        public LykkeLimitOrdersSubscriber(
            ILogFactory logFactory,
            ILykkeTradeService lykkeTradeService,
            ISettingsService settingsService,
            RabbitMqSettings settings
            )
        {
            _logFactory = logFactory;
            _lykkeTradeService = lykkeTradeService;
            _settingsService = settingsService;
            _settings = settings;
            _log = logFactory.CreateLog(this);
        }

        public void Start()
        {
            // NOTE: Read https://github.com/LykkeCity/Lykke.RabbitMqDotNetBroker/blob/master/README.md to learn
            // about RabbitMq subscriber configuration

            var settings = RabbitMqSubscriptionSettings
                .ForSubscriber(_settings.ConnectionString, _settings.ExchangeName, "lp3")
                .MakeDurable();

            _subscriber = new RabbitMqSubscriber<LimitOrders>(
                    _logFactory,
                    settings,
                    new ResilientErrorHandlingStrategy(
                        _logFactory,
                        settings,
                        TimeSpan.FromSeconds(10),
                        next: new DeadQueueErrorHandlingStrategy(_logFactory, settings)))
                .SetMessageDeserializer(new JsonMessageDeserializer<LimitOrders>())
                .Subscribe(ProcessMessageAsync)
                .CreateDefaultBinding()
                .Start();
        }

        private async Task ProcessMessageAsync(LimitOrders limitOrders)
        {
            try
            {
                if (limitOrders.Orders == null || limitOrders.Orders.Count == 0)
                    return;

                string walletId = _settingsService.GetWalletId();

                if (string.IsNullOrEmpty(walletId))
                    return;

                List<LimitOrderWithTrades> clientLimitOrders = limitOrders.Orders
                    .Where(o => o.Order?.ClientId == walletId)
                    .Where(o => o.Trades?.Count > 0)
                    .ToList();

                if (!clientLimitOrders.Any())
                    return;
                
                _log.Info("LimitOrders received", context: $"{clientLimitOrders.ToJson()}");
                

                IReadOnlyCollection<Trade> trades = ExtractTrades(clientLimitOrders);

                if (trades.Any())
                {
                    await _lykkeTradeService.HandleAsync(trades);
                }
            }
            catch (Exception exception)
            {
                _log.Error(exception, "An error occurred during processing trades", limitOrders);
            }
            
            
            
        }
        
        private static IReadOnlyCollection<Trade> ExtractTrades(IEnumerable<LimitOrderWithTrades> limitOrders)
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

        public void Dispose()
        {
            _subscriber?.Dispose();
        }

        public void Stop()
        {
            _subscriber?.Stop();
        }
    }
}

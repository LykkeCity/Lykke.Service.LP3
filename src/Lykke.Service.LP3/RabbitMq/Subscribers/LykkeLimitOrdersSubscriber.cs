using System;
using System.Threading.Tasks;
using Autofac;
using Common;
using Lykke.Common.Log;
using Lykke.MatchingEngine.ExchangeModels;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.LP3.Domain.Services;

namespace Lykke.Service.LP3.RabbitMq.Subscribers
{
    public class LykkeLimitOrdersSubscriber : IStartable, IStopable
    {
        private readonly ILogFactory _logFactory;
        private readonly ILykkeTradeService _lykkeTradeService;
        private readonly string _connectionString;
        private readonly string _exchangeName;
        private RabbitMqSubscriber<LimitOrders> _subscriber;

        public LykkeLimitOrdersSubscriber(
            ILogFactory logFactory,
            ILykkeTradeService lykkeTradeService,
            string connectionString,
            string exchangeName)
        {
            _logFactory = logFactory;
            _lykkeTradeService = lykkeTradeService;
            _connectionString = connectionString;
            _exchangeName = exchangeName;
        }

        public void Start()
        {
            // NOTE: Read https://github.com/LykkeCity/Lykke.RabbitMqDotNetBroker/blob/master/README.md to learn
            // about RabbitMq subscriber configuration

            var settings = RabbitMqSubscriptionSettings
                .ForSubscriber(_connectionString, _exchangeName, "lp3")
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

        private Task ProcessMessageAsync(LimitOrders arg)
        {
            return _lykkeTradeService.HandleAsync(arg);
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

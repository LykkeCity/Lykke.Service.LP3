using System;
using System.Threading.Tasks;
using Autofac;
using Common;
using Lykke.Common.Log;
using Lykke.MatchingEngine.ExchangeModels;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.LP3.Settings;

namespace Lykke.Service.LP3.RabbitMq.Subscribers
{
    public class LykkeOrderBookSubscriber : IStartable, IStopable
    {
        private readonly ILogFactory _logFactory;
        private readonly RabbitMqSettings _settings;
        private RabbitMqSubscriber<OrderBook> _subscriber;

        public LykkeOrderBookSubscriber(
            ILogFactory logFactory,
            RabbitMqSettings settings)
        {
            _logFactory = logFactory;
            _settings = settings;
        }

        public void Start()
        {
            // NOTE: Read https://github.com/LykkeCity/Lykke.RabbitMqDotNetBroker/blob/master/README.md to learn
            // about RabbitMq subscriber configuration

            var settings = RabbitMqSubscriptionSettings
                .ForSubscriber(_settings.ConnectionString, _settings.ExchangeName, "lp3");

            _subscriber = new RabbitMqSubscriber<OrderBook>(
                    _logFactory,
                    settings,
                    new ResilientErrorHandlingStrategy(
                        _logFactory,
                        settings,
                        TimeSpan.FromSeconds(10),
                        next: new DeadQueueErrorHandlingStrategy(_logFactory, settings)))
                .SetMessageDeserializer(new JsonMessageDeserializer<OrderBook>())
                .Subscribe(ProcessMessageAsync)
                .CreateDefaultBinding()
                .Start();
        }

        private Task ProcessMessageAsync(OrderBook arg)
        {
            // TODO: handle it
            
            return Task.CompletedTask;
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

using System;
using System.Threading.Tasks;
using Autofac;
using Common;
using Lykke.Common.Log;
using Lykke.MatchingEngine.ExchangeModels;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;

namespace Lykke.Service.LP3.RabbitMq.Subscribers
{
    public class LykkeOrderBookSubscriber : IStartable, IStopable
    {
        private readonly ILogFactory _logFactory;
        private readonly string _connectionString;
        private readonly string _exchangeName;
        private RabbitMqSubscriber<OrderBook> _subscriber;

        public LykkeOrderBookSubscriber(
            ILogFactory logFactory,
            string connectionString,
            string exchangeName)
        {
            _logFactory = logFactory;
            _connectionString = connectionString;
            _exchangeName = exchangeName;
        }

        public void Start()
        {
            // NOTE: Read https://github.com/LykkeCity/Lykke.RabbitMqDotNetBroker/blob/master/README.md to learn
            // about RabbitMq subscriber configuration

            var settings = RabbitMqSubscriptionSettings
                .ForSubscriber(_connectionString, _exchangeName, "lp3");

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

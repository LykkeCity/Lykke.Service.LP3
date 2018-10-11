using System;
using System.Threading.Tasks;
using Autofac;
using AutoMapper;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.LP3.Domain;
using Lykke.Service.LP3.Domain.Services;
using Lykke.Service.LP3.Settings;
using ExternalOrderBook = Lykke.Common.ExchangeAdapter.Contracts.OrderBook;
using ExternalTickPrice = Lykke.Common.ExchangeAdapter.Contracts.TickPrice;
using DomainTickPrice = Lykke.Service.LP3.Domain.TickPrice;

namespace Lykke.Service.LP3.RabbitMq.Subscribers
{
    public class ExternalOrderBookSubscriber : IStartable, IStopable
    {
        private readonly ILogFactory _logFactory;
        private readonly RabbitMqSettings _settings;
        private readonly string _exchangeName;
        private readonly ICrossRateService _crossRateService;
        private RabbitMqSubscriber<ExternalOrderBook> _subscriber;
        private readonly ILog _log;

        public ExternalOrderBookSubscriber(
            ILogFactory logFactory,
            RabbitMqSettings settings,
            string exchangeName,
            ICrossRateService crossRateService)
        {
            _logFactory = logFactory;
            _settings = settings;
            _exchangeName = exchangeName;
            _crossRateService = crossRateService;
            _log = logFactory.CreateLog(this);
        }

        public void Start()
        {
            // NOTE: Read https://github.com/LykkeCity/Lykke.RabbitMqDotNetBroker/blob/master/README.md to learn
            // about RabbitMq subscriber configuration
            
            if (_exchangeName.Equals(Consts.LykkeExchangeName, StringComparison.CurrentCultureIgnoreCase))
            {
                _log.Warning("Lykke exchange is set as ExternalOrderBook");
                return;
            }

            var settings = RabbitMqSubscriptionSettings
                .ForSubscriber(_settings.ConnectionString, _settings.ExchangeName, "lp3");

            _subscriber = new RabbitMqSubscriber<ExternalOrderBook>(
                    _logFactory,
                    settings,
                    new ResilientErrorHandlingStrategy(
                        _logFactory,
                        settings,
                        TimeSpan.FromSeconds(10),
                        next: new DeadQueueErrorHandlingStrategy(_logFactory, settings)))
                .SetMessageDeserializer(new JsonMessageDeserializer<ExternalOrderBook>())
                .Subscribe(ProcessMessageAsync)
                .CreateDefaultBinding()
                .Start();
        }

        private async Task ProcessMessageAsync(ExternalOrderBook orderBook)
        {
            var externalTickPrice = ExternalTickPrice.FromOrderBook(orderBook);
            var domainTickPrice = Mapper.Map<DomainTickPrice>(externalTickPrice);
            
            await _crossRateService.HandleAsync(domainTickPrice);
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

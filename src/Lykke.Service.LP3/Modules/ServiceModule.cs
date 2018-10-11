using System.Linq;
using System.Net;
using Autofac;
using Common;
using Lykke.Sdk;
using Lykke.Service.Assets.Client;
using Lykke.Service.Balances.Client;
using Lykke.Service.LP3.AzureRepositories.Infrastructure;
using Lykke.Service.LP3.PeriodicalHandlers;
using Lykke.Service.LP3.RabbitMq.Subscribers;
using Lykke.Service.LP3.Services;
using Lykke.Service.LP3.Settings;
using Lykke.Service.LP3.Settings.Clients;
using Lykke.SettingsReader;

namespace Lykke.Service.LP3.Modules
{
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _appSettings;

        public ServiceModule(IReloadingManager<AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule(new DomainServices.AutofacModule(
                _appSettings.CurrentValue.LP3Service.WalletId,
                _appSettings.CurrentValue.LP3Service.ExternalExchanges?.Select(x => x.Name)));

            builder.RegisterModule(new AutofacModule(_appSettings.Nested(x => x.LP3Service.Db.DataConnectionString)));
            
            
            builder.RegisterType<StartupManager>()
                .As<IStartupManager>()
                .SingleInstance();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>()
                .AutoActivate()
                .SingleInstance();
            
            RegisterClients(builder);
            
            RegisterRabbitMqSubscribers(builder);
            
            RegisterPeriodicalHandlers(builder);
        }
        
        private void RegisterClients(ContainerBuilder builder)
        {
            builder.RegisterAssetsClient(_appSettings.CurrentValue.AssetsServiceClient);

            builder.RegisterBalancesClient(_appSettings.CurrentValue.BalancesServiceClient);

            RegisterMeClient(builder);
        }

        private void RegisterMeClient(ContainerBuilder builder)
        {
            MatchingEngineClientSettings matchingEngineClientSettings = _appSettings.CurrentValue.MatchingEngineClient;

            if (!IPAddress.TryParse(matchingEngineClientSettings.IpEndpoint.Host, out var address))
                address = Dns.GetHostAddressesAsync(matchingEngineClientSettings.IpEndpoint.Host).Result[0];

            var endPoint = new IPEndPoint(address, matchingEngineClientSettings.IpEndpoint.Port);

            builder.RegisgterMeClient(endPoint);
        }
        
        private void RegisterRabbitMqSubscribers(ContainerBuilder builder)
        {
            // TODO: You should register each subscriber in DI container as IStartable singleton and autoactivate it

            builder.RegisterType<LykkeLimitOrdersSubscriber>()
                .As<IStartable>()
                .SingleInstance()
                .WithParameter(TypedParameter.From(_appSettings.CurrentValue.LP3Service.Rabbit.Subscribers.LykkeOrders));
            
            builder.RegisterType<LykkeOrderBookSubscriber>()
                .As<IStartable>()
                .SingleInstance()
                .WithParameter(TypedParameter.From(_appSettings.CurrentValue.LP3Service.Rabbit.Subscribers.LykkeOrderBooks));

            foreach (var externalExchangeSettings in _appSettings.CurrentValue.LP3Service.ExternalExchanges)
            {
                builder.RegisterType<ExternalOrderBookSubscriber>()
                    .AsSelf()
                    .WithParameter(TypedParameter.From(externalExchangeSettings.Name))
                    .WithParameter(TypedParameter.From(externalExchangeSettings.Rabbit))
                    .Named<ExternalOrderBookSubscriber>(externalExchangeSettings.Name)
                    .As<IStartable>()
                    .SingleInstance();
            }
        }
        
        private void RegisterPeriodicalHandlers(ContainerBuilder builder)
        {
            // TODO: You should register each periodical handler in DI container as IStartable singleton and autoactivate it

            builder.RegisterType<RecreateOrdersPeriodicalHandler>()
                .As<IStartable>()
                .As<IStopable>()
                .SingleInstance();
        }
    }
}

using System.Collections.Generic;
using Autofac;
using Lykke.Service.LP3.Domain.Exchanges;
using Lykke.Service.LP3.Domain.Services;
using Lykke.Service.LP3.DomainServices.Exchanges;

namespace Lykke.Service.LP3.DomainServices
{
    public class AutofacModule : Module
    {
        private readonly string _walletId;
        private readonly IEnumerable<string> _availableExternalExchanges;

        public AutofacModule(string walletId, IEnumerable<string> availableExternalExchanges)
        {
            _walletId = walletId;
            _availableExternalExchanges = availableExternalExchanges;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SettingsService>()
                .As<ISettingsService>()
                .WithParameter(new NamedParameter("walletId", _walletId))
                .WithParameter(TypedParameter.From(_availableExternalExchanges))
                .SingleInstance();

            builder.RegisterType<LykkeExchange>()
                .As<ILykkeExchange>()
                .As<IStartable>()
                .SingleInstance();

            builder.RegisterType<LykkeTradeService>()
                .As<ILykkeTradeService>()
                .SingleInstance();

            builder.RegisterType<LevelsService>()
                .As<ILevelsService>()
                .As<IStartable>()
                .SingleInstance()
                .AutoActivate();

            builder.RegisterType<Lp3Service>()
                .As<ILp3Service>()
                .As<IStartable>()
                .SingleInstance()
                .AutoActivate();

            builder.RegisterType<AdditionalVolumeService>()
                .As<IAdditionalVolumeService>()
                .SingleInstance();

            builder.RegisterType<OrdersConverter>()
                .As<IOrdersConverter>()
                .SingleInstance();

            builder.RegisterType<TradesConverter>()
                .As<ITradesConverter>()
                .SingleInstance();
            
            builder.RegisterType<CrossRateService>()
                .As<ICrossRateService>()
                .SingleInstance();
        }
    }
}

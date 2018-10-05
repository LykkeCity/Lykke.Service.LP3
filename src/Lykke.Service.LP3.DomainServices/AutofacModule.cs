using Autofac;
using Lykke.Service.LP3.Domain.Exchanges;
using Lykke.Service.LP3.Domain.Services;
using Lykke.Service.LP3.DomainServices.Exchanges;

namespace Lykke.Service.LP3.DomainServices
{
    public class AutofacModule : Module
    {
        private readonly string _walletId;

        public AutofacModule(string walletId)
        {
            _walletId = walletId;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SettingsService>()
                .As<ISettingsService>()
                .WithParameter(new NamedParameter("walletId", _walletId))
                .SingleInstance();

            builder.RegisterType<LykkeExchange>()
                .As<ILykkeExchange>()
                .SingleInstance();

            builder.RegisterType<LykkeTradeService>()
                .As<ILykkeTradeService>()
                .SingleInstance();

            builder.RegisterType<Trader>()
                .As<ITrader>()
                .SingleInstance();
        }
    }
}

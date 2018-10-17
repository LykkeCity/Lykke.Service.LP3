using System;
using System.Collections.Generic;
using Autofac;
using Lykke.Service.LP3.Domain.Exchanges;
using Lykke.Service.LP3.Domain.Services;
using Lykke.Service.LP3.Domain.Settings;
using Lykke.Service.LP3.DomainServices.Assets;
using Lykke.Service.LP3.DomainServices.Balances;
using Lykke.Service.LP3.DomainServices.Exchanges;
using Lykke.Service.LP3.DomainServices.Timers;

namespace Lykke.Service.LP3.DomainServices
{
    public class AutofacModule : Module
    {
        private readonly string _walletId;
        private readonly IEnumerable<string> _availableExternalExchanges;
        private readonly IReadOnlyCollection<AssetMapping> _assetMappings;
        private readonly TimerSettings _timerSettings;

        public AutofacModule(
            string walletId, 
            IEnumerable<string> availableExternalExchanges,
            IReadOnlyCollection<AssetMapping> assetMappings,
            TimerSettings timerSettings)
        {
            _walletId = walletId;
            _availableExternalExchanges = availableExternalExchanges;
            _assetMappings = assetMappings;
            _timerSettings = timerSettings;
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
                .WithParameter(new NamedParameter("walletId", _walletId))
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

            builder.RegisterType<LykkeAssetsService>()
                .As<IAssetsService>()
                .SingleInstance();

            builder.RegisterType<AssetLinkService>()
                .As<IAssetLinkService>()
                .WithParameter(TypedParameter.From(_assetMappings))
                .SingleInstance();

            builder.RegisterType<BalanceService>()
                .As<IBalanceService>()
                .SingleInstance();

            RegisterTimers(builder);
        }

        private void RegisterTimers(ContainerBuilder builder)
        {
            builder.RegisterType<BalancesTimer>()
                .AsSelf()
                .WithParameter(TypedParameter.From(_timerSettings.BalanceTimer));
        }
    }
}

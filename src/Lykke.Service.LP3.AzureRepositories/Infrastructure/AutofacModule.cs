using Autofac;
using AzureStorage.Tables;
using Lykke.Common.Log;
using Lykke.Service.LP3.Domain.Repositories;
using Lykke.SettingsReader;

namespace Lykke.Service.LP3.AzureRepositories.Infrastructure
{
    public class AutofacModule : Module
    {
        private readonly IReloadingManager<string> _connectionString;

        public AutofacModule(IReloadingManager<string> connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void Load(ContainerBuilder builder)
        {
            const string settingsTableName = "Settings";
            const string tradesTableName = "Trades";
            
            builder.Register(container => new LevelRepository(
                    AzureTableStorage<LevelEntity>.Create(_connectionString,
                        settingsTableName, container.Resolve<ILogFactory>())))
                .As<ILevelRepository>()
                .SingleInstance();
            
            builder.Register(container => new InitialPriceRepository(
                    AzureTableStorage<InitialPriceEntity>.Create(_connectionString,
                        settingsTableName, container.Resolve<ILogFactory>())))
                .As<IInitialPriceRepository>()
                .SingleInstance();
            
            builder.Register(container => new BaseAssetPairSettingsRepository(
                    AzureTableStorage<BaseAssetPairSettingsEntity>.Create(_connectionString,
                        settingsTableName, container.Resolve<ILogFactory>())))
                .As<IBaseAssetPairSettingsRepository>()
                .SingleInstance();
            
            builder.Register(container => new AdditionalVolumeSettingsRepository(
                    AzureTableStorage<AdditionalVolumeSettingsEntity>.Create(_connectionString,
                        settingsTableName, container.Resolve<ILogFactory>())))
                .As<IAdditionalVolumeSettingsRepository>()
                .SingleInstance();
            
            builder.Register(container => new TradeRepository(
                    AzureTableStorage<TradeEntity>.Create(_connectionString,
                        tradesTableName, container.Resolve<ILogFactory>())))
                .As<ITradeRepository>()
                .SingleInstance();
        }
    }
}

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
            
            builder.Register(container => new LevelsSettingsRepository(
                    AzureTableStorage<LevelSettingsEntity>.Create(_connectionString,
                        settingsTableName, container.Resolve<ILogFactory>())))
                .As<ILevelsSettingsRepository>()
                .SingleInstance();
        }
    }
}

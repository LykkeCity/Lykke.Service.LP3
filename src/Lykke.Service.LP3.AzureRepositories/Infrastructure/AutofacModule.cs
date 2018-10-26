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
            const string orderBookTradersTableName = "OrderBookTraders";
            const string tradesTableName = "Trades";
            const string limitOrdersTableName = "LimitOrders";
            const string assetPairsTableName = "AssetPairs";
            
            builder.Register(container => new TradeRepository(
                    AzureTableStorage<TradeEntity>.Create(_connectionString,
                        tradesTableName, container.Resolve<ILogFactory>())))
                .As<ITradeRepository>()
                .SingleInstance();

            builder.Register(container => new OrderBookTraderRepository(
                    AzureTableStorage<OrderBookEntity>.Create(_connectionString,
                        orderBookTradersTableName, container.Resolve<ILogFactory>())))
                .As<IOrderBookTraderRepository>()
                .SingleInstance();

            builder.Register(container => new LimitOrderRepository(
                    AzureTableStorage<LimitOrderEntity>.Create(_connectionString,
                        limitOrdersTableName, container.Resolve<ILogFactory>())))
                .As<ILimitOrderRepository>()
                .SingleInstance();

            builder.Register(container => new AssetPairRepository(
                    AzureTableStorage<AssetPairEntity>.Create(_connectionString,
                        assetPairsTableName, container.Resolve<ILogFactory>())))
                .As<IAssetPairRepository>()
                .SingleInstance();
        }
    }
}

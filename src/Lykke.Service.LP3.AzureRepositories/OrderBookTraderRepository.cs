using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.LP3.Domain.Repositories;
using Lykke.Service.LP3.Domain.TradingAlgorithm;

namespace Lykke.Service.LP3.AzureRepositories
{
    public class OrderBookTraderRepository : IOrderBookTraderRepository
    {
        private readonly INoSQLTableStorage<OrderBookEntity> _storage;

        public OrderBookTraderRepository(INoSQLTableStorage<OrderBookEntity> storage)
        {
            _storage = storage;
        }

        public async Task AddAsync(OrderBookTrader orderBookTrader)
        {
            var entity = new OrderBookEntity(GetPartitionKey(), GetRowKey(orderBookTrader.AssetPairId));

            Mapper.Map(orderBookTrader, entity);

            await _storage.InsertAsync(entity);
        }
        
        public async Task<IReadOnlyList<OrderBookTrader>> GetAllAsync()
        {
            IEnumerable<OrderBookEntity> data = await _storage.GetDataAsync(GetPartitionKey());

            return Mapper.Map<List<OrderBookTrader>>(data);
        }

        public async Task UpdateAsync(OrderBookTrader orderBookTrader)
        {
            var entity = new OrderBookEntity(GetPartitionKey(), GetRowKey(orderBookTrader.AssetPairId));

            Mapper.Map(orderBookTrader, entity);

            await _storage.InsertOrMergeAsync(entity);
        }

        public async Task DeleteAsync(string assetPairId)
        {
            await _storage.DeleteAsync(GetPartitionKey(), GetRowKey(assetPairId));
        }

        private string GetPartitionKey() => "OrderBook";

        private string GetRowKey(string assetPairId) => assetPairId.ToUpperInvariant();
    }
}

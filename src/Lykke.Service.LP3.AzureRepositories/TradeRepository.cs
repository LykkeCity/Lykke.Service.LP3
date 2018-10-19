using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Lykke.Service.LP3.Domain.Orders;
using Lykke.Service.LP3.Domain.Repositories;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.LP3.AzureRepositories
{
    public class TradeRepository : ITradeRepository
    {
        private readonly INoSQLTableStorage<TradeEntity> _storage;

        public TradeRepository(INoSQLTableStorage<TradeEntity> storage)
        {
            _storage = storage;
        }

        public async Task<IReadOnlyList<Trade>> GetAsync(DateTime startDate, DateTime endDate)
        {
            var filter = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition(nameof(TradeEntity.PartitionKey), QueryComparisons.GreaterThan,
                    GetPartitionKey(startDate)),
                TableOperators.And,
                TableQuery.GenerateFilterCondition(nameof(TradeEntity.PartitionKey), QueryComparisons.LessThan,
                    GetPartitionKey(endDate)));

            var query = new TableQuery<TradeEntity>().Where(filter);
            
            IEnumerable<TradeEntity> entities = await _storage.WhereAsync(query);

            return Mapper.Map<List<Trade>>(entities);
        }
        
        public async Task InsertAsync(Trade trade)
        {
            var entity = new TradeEntity(GetPartitionKey(trade.Time), GetRowKey(trade.Id));

            Mapper.Map(trade, entity);
            
            await _storage.InsertAsync(entity);
        }

        private static string GetPartitionKey(DateTime date)
            => $"{date:yyyy-MM-ddTHH:mm}";

        private static string GetRowKey(string tradeId)
            => tradeId;
    }
}

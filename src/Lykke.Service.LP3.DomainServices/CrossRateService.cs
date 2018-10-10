using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Lykke.MatchingEngine.ExchangeModels;
using Lykke.Service.LP3.Domain.Repositories;
using Lykke.Service.LP3.Domain.Services;

namespace Lykke.Service.LP3.DomainServices
{
    public class CrossRateService : ICrossRateService
    {
        private readonly ICrossRateRepository _crossRateRepository;

        private readonly ConcurrentDictionary<string, decimal> _bestBids = new ConcurrentDictionary<string, decimal>();
        private readonly ConcurrentDictionary<string, decimal> _bestAsks = new ConcurrentDictionary<string, decimal>();

        public CrossRateService(ICrossRateRepository crossRateRepository)
        {
            _crossRateRepository = crossRateRepository;
        }

        public async Task HandleAsync(string exchange, OrderBook orderBook)
        {
            if (orderBook.IsBuy)
            {
                var bestBid = (decimal)orderBook.Prices.OrderBy(x => x.Price).Last().Price;
                _bestBids.AddOrUpdate(Key(exchange, orderBook.AssetPair), bestBid, (key, price) => bestBid);
            }
            else
            {
                var bestAsk = (decimal)orderBook.Prices.OrderBy(x => x.Price).First().Price;
                _bestAsks.AddOrUpdate(Key(exchange, orderBook.AssetPair), bestAsk, (key, price) => bestAsk);
            }

            // TODO: save
        }

        private string Key(string exchange, string assetPair) => $"{assetPair}@{exchange}".ToLowerInvariant();
    }
}

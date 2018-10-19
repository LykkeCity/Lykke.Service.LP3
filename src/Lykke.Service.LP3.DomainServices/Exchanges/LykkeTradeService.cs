using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.LP3.Domain.Repositories;
using Lykke.Service.LP3.Domain.Services;
using Trade = Lykke.Service.LP3.Domain.Orders.Trade;

namespace Lykke.Service.LP3.DomainServices.Exchanges
{
    [UsedImplicitly]
    public class LykkeTradeService : ILykkeTradeService
    {
        private readonly ITradeRepository _tradeRepository;
        private readonly ILp3Service _lp3Service;
        private readonly ILog _log;

        public LykkeTradeService(
            ITradeRepository tradeRepository,
            ILp3Service lp3Service,
            ILogFactory logFactory)
        {
            _tradeRepository = tradeRepository;
            _lp3Service = lp3Service;
            _log = logFactory.CreateLog(this);
        }

        public Task<IReadOnlyList<Trade>> GetAsync(DateTime startDate, DateTime endDate)
        {
            return _tradeRepository.GetAsync(startDate, endDate);
        }

        public async Task HandleAsync(IReadOnlyCollection<Trade> trades)
        {
            await SaveTrades(trades);
            await _lp3Service.HandleTradesAsync(trades);
        }

        private async Task SaveTrades(IReadOnlyCollection<Trade> trades)
        {
            foreach (var trade in trades)
            {
                try
                {
                    await _tradeRepository.InsertAsync(trade);
                }
                catch (Exception e)
                {
                    _log.Error(e, $"Error on saving trade {trade.ToJson()}");
                }
            }
        }
    }
}

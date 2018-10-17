using System;
using System.Threading;
using System.Threading.Tasks;
using Lykke.Common.Log;
using Lykke.Service.LP3.Domain.Services;

namespace Lykke.Service.LP3.DomainServices.Timers
{
    public class BalancesTimer : Timer
    {
        private readonly IBalanceService _balanceService;
        private readonly TimeSpan _period;
        public BalancesTimer(
            IBalanceService balanceService,
            TimeSpan period,
            ILogFactory logFactory)
        {
            _balanceService = balanceService;
            _period = period;
            Log = logFactory.CreateLog(this);
        }

        protected override Task<TimeSpan> GetDelayAsync()
        {
            return Task.FromResult(_period);
        }

        protected override Task OnExecuteAsync(CancellationToken cancellation)
        {
            return _balanceService.UpdateBalancesAsync();
        }
    }
}

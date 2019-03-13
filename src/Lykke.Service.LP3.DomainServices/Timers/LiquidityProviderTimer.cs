using System;
using System.Threading;
using System.Threading.Tasks;
using Lykke.Common.Log;
using Lykke.Service.LP3.Domain.Services;

namespace Lykke.Service.LP3.DomainServices.Timers
{
    public class LiquidityProviderTimer : Timer
    {
        private readonly ILp3Service _liquidityProvider;
        private readonly TimeSpan _interval;

        public LiquidityProviderTimer(
            ILp3Service liquidityProvider,
            TimeSpan interval,
            ILogFactory logFactory)
        {
            _liquidityProvider = liquidityProvider;
            _interval = interval;
            Log = logFactory.CreateLog(this);
        }

        protected override Task<TimeSpan> GetDelayAsync()
        {
            return Task.FromResult(_interval);
        }

        protected override Task OnExecuteAsync(CancellationToken cancellation)
        {
            return _liquidityProvider.ApplyOrderBooksAsync();
        }
    }
}

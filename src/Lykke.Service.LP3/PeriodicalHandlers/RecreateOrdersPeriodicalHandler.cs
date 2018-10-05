using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Common;
using Lykke.Common.Log;
using Lykke.Service.LP3.Domain.Services;

namespace Lykke.Service.LP3.PeriodicalHandlers
{
    public class RecreateOrdersPeriodicalHandler : IStartable, IStopable
    {
        private readonly ILp3Service _lp3Service;
        private readonly TimerTrigger _timerTrigger;
        
        public RecreateOrdersPeriodicalHandler(ILogFactory logFactory,
            ILp3Service lp3Service)
        {
            _lp3Service = lp3Service;
            // TODO: Sometimes, it is enough to hardcode the period right here, but sometimes it's better to move it to the settings.
            // Choose the simplest and sufficient solution
            _timerTrigger = new TimerTrigger(nameof(RecreateOrdersPeriodicalHandler), TimeSpan.FromMinutes(1), logFactory);
            _timerTrigger.Triggered += Execute;
        }

        public void Start()
        {
            _timerTrigger.Start();
        }
        
        public void Stop()
        {
            _timerTrigger.Stop();
        }

        public void Dispose()
        {
            _timerTrigger.Stop();
            _timerTrigger.Dispose();
        }

        private async Task Execute(ITimerTrigger timer, TimerTriggeredHandlerArgs args, CancellationToken cancellationToken)
        {
            await _lp3Service.HandleTimerAsync();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Sdk;
using Lykke.Service.LP3.DomainServices.Timers;

namespace Lykke.Service.LP3.Services
{
    // NOTE: Sometimes, shutdown process should be expressed explicitly. 
    // If this is your case, use this class to manage shutdown.
    // For example, sometimes some state should be saved only after all incoming message processing and 
    // all periodical handler was stopped, and so on.
    public class ShutdownManager : IShutdownManager
    {
        private readonly BalancesTimer _balancesTimer;
        private readonly ILog _log;
        private readonly IEnumerable<IStopable> _items;

        public ShutdownManager(
            BalancesTimer balancesTimer,
            ILogFactory logFactory,
            IEnumerable<IStopable> items)
        {
            _balancesTimer = balancesTimer;
            _log = logFactory.CreateLog(this);
            _items = items;
        }

        public async Task StopAsync()
        {
            _balancesTimer.Stop();

            foreach (var item in _items)
            {
                try
                {
                    item.Stop();
                }
                catch (Exception ex)
                {
                    _log.Warning($"Unable to stop {item.GetType().Name}", ex);
                }
            }

            await Task.CompletedTask;
        }
    }
}

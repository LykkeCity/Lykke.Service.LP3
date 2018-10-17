using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Sdk;
using Lykke.Service.LP3.DomainServices.Timers;

namespace Lykke.Service.LP3.Services
{
    // NOTE: Sometimes, startup process which is expressed explicitly is not just better, 
    // but the only way. If this is your case, use this class to manage startup.
    // For example, sometimes some state should be restored before any periodical handler will be started, 
    // or any incoming message will be processed and so on.
    // Do not forget to remove As<IStartable>() and AutoActivate() from DI registartions of services, 
    // which you want to startup explicitly.

    public class StartupManager : IStartupManager
    {
        private readonly BalancesTimer _balancesTimer;
        private readonly ILog _log;

        public StartupManager(
            BalancesTimer balancesTimer,
            ILogFactory logFactory)
        {
            _balancesTimer = balancesTimer;
            _log = logFactory.CreateLog(this);
        }

        public async Task StartAsync()
        {
            _balancesTimer.Start();

            await Task.CompletedTask;
        }
    }
}

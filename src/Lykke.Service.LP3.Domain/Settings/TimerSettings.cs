using System;

namespace Lykke.Service.LP3.Domain.Settings
{
    public class TimerSettings
    {
        public TimeSpan BalanceTimer { get; set; }
        
        public TimeSpan LiquidityProvider { get; set; }
    }
}

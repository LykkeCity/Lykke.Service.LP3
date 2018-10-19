using System;

namespace Lykke.Service.LP3.Domain
{
    public static class Consts
    {
        public static TimeSpan LockTimeOut = TimeSpan.FromSeconds(10);

        public static string LykkeExchangeName = "Lykke";
        
        public static TimeSpan RetryPlacingOrdersPeriod = TimeSpan.FromSeconds(10);
        
        public static TimeSpan MatchingEngineTimeout = TimeSpan.FromSeconds(7);
    }
}

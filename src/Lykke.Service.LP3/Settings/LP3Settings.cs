using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Lykke.Service.LP3.Domain.Settings;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.LP3.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class LP3Settings
    {
        public DbSettings Db { get; set; }
        
        public string WalletId { get; set; }
        
        public RabbitSettingsGroup Rabbit { get; set; }
        
        public ExternalExchangeSettings[] ExternalExchanges { get; set; }

        [Optional] 
        public TimeSpan OrdersCreationPeriod { get; set; } = TimeSpan.FromSeconds(30);

        public IReadOnlyCollection<AssetMapping> AssetMappings { get; set; }

        public TimerSettings Timers { get; set; }
    }
}

using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.LP3.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class LP3Settings
    {
        public DbSettings Db { get; set; }
        
        public string WalletId { get; set; }
        
        public RabbitSettingsGroup Rabbit { get; set; }
    }
}

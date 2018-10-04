using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.LP3.Settings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }
    }
}

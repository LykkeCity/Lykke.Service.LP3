using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.LP3.Settings.JobSettings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }
    }
}

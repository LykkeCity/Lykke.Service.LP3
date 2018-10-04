using Lykke.Job.LP3.Settings.JobSettings;
using Lykke.Sdk.Settings;

namespace Lykke.Job.LP3.Settings
{
    public class AppSettings : BaseAppSettings
    {
        public LP3JobSettings LP3Job { get; set; }
    }
}

using JetBrains.Annotations;

namespace Lykke.Service.LP3.Settings.Clients
{
    [UsedImplicitly]
    public class IpEndpointSettings
    {
        public string Host { get; set; }

        public int Port { get; set; }
    }
}

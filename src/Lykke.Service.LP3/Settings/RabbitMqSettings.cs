using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.LP3.Settings
{
    public class RabbitMqSettings
    {
        [AmqpCheck]
        public string ConnectionString { get; set; }

        public string ExchangeName { get; set; }
    }
}

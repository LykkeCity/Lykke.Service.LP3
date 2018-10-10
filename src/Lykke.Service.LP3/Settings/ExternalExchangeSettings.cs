namespace Lykke.Service.LP3.Settings
{
    public class ExternalExchangeSettings
    {
        public string Name { get; set; }

        public RabbitMqSettings Rabbit { get; set; }
    }
}

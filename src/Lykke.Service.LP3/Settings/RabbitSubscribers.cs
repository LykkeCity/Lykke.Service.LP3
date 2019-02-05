using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.LP3.Settings
{
    public class RabbitSubscribers
    {
        public RabbitSubscribers()
        {
            LykkeOrdersQueueSyffix = "lp3";
        }

        public RabbitMqSettings LykkeOrders { get; set; }

        [Optional]
        public string LykkeOrdersQueueSyffix { get; set; }
    }
}

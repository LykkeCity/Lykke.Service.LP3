namespace Lykke.Service.LP3.Settings
{
    public class RabbitSubscribers
    {
        public RabbitMqSettings LykkeOrders { get; set; }
        
        public RabbitMqSettings LykkeOrderBooks { get; set; }
    }
}

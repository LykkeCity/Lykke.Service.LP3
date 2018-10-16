namespace Lykke.Service.LP3.Client.Models.Orders
{
    public class DependentLimitOrderModel : LimitOrderModel
    {
        public string Description { get; set; }
        
        public LimitOrderModel BaseLimitOrder { get; set; }
        
        public TickPriceModel CrossTickPrice { get; set; }
    }
}

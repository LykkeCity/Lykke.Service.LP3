namespace Lykke.Service.LP3.Client.Models.Orders
{
    public class LimitOrderPostModel
    {
        public string AssetPairId { get; set; }
        
        public decimal Price { get; set; }
        
        public decimal Volume { get; set; }
        
        public decimal Number { get; set; }
        
        public TradeType TradeType { get; set; }
    }
}

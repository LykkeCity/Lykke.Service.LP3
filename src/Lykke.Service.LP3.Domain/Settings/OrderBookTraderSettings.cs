namespace Lykke.Service.LP3.Domain.Settings
{
    public class OrderBookTraderSettings
    {
        public string AssetPairId { get; set; }
        
        public bool IsEnabled { get; set; }
        
        public decimal InitialPrice { get; set; }
        
        public decimal Delta { get; set; }
        
        public decimal Volume { get; set; }
        
        public int Count { get; set; }
        
        public int MinCountOrderInMarket { get; set; }
        
        public int AddedCountOrdersInMarket { get; set; }
    }
}

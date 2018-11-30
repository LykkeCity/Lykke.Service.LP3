namespace Lykke.Service.LP3.Client.Models.Settings
{
    public class OrderBookTraderSettingsModel
    {
        public string AssetPairId { get; set; }
        
        public bool IsEnabled { get; set; }
        
        public decimal InitialPrice { get; set; }
        
        public decimal Delta { get; set; }
        
        public decimal Volume { get; set; }
        
        public int Count { get; set; }
        
        public int CountInMarket { get; set; }
    }
}

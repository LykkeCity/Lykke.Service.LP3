namespace Lykke.Service.LP3.Client.Models
{
    public class OrderBookTraderModel
    {
        public string AssetPairId { get; set; }
        
        public bool IsEnabled { get; set; }
        
        public decimal InitialPrice { get; set; }
        
        public decimal Delta { get; set; }
        
        public decimal Volume { get; set; }
        
        public int Count { get; set; }
        
        public decimal Inventory { get; set; }
        
        public decimal OppositeInventory { get; set; }
    }
}

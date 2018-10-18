namespace Lykke.Service.LP3.Client.Models
{
    public class OrderBookTraderModel
    {
        public string AssetPairId { get; set; }
        
        public bool IsEnabled { get; set; }
        
        public decimal ReferencePrice { get; set; }
        
        public decimal LevelDelta { get; set; }
        
        public decimal LevelOriginalVolume { get; set; }
        
        public decimal AdditionalOrdersDelta { get; set; }
        
        public decimal AdditionalOrdersVolume { get; set; }
        
        public int AdditionalOrdersCount { get; set; }
        
        public decimal Inventory { get; set; }
        
        public decimal OppositeInventory { get; set; }
        
        
    }
}

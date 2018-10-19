namespace Lykke.Service.LP3.Client.Models.Settings
{
    public class OrderBookTraderSettingsModel
    {
        public string AssetPairId { get; set; }
        
        public bool IsEnabled { get; set; }
        
        public decimal ReferencePrice { get; set; }
        
        public decimal LevelDelta { get; set; }
        
        public decimal LevelOriginalVolume { get; set; }
        
        public decimal AdditionalOrdersDelta { get; set; }
        
        public decimal AdditionalOrdersVolume { get; set; }
        
        public int AdditionalOrdersCount { get; set; }
    }
}

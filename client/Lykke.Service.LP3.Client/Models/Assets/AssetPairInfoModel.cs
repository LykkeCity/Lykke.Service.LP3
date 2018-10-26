namespace Lykke.Service.LP3.Client.Models.Assets
{
    public class AssetPairInfoModel
    {
        public string AssetPairId { get; set; }
        
        public int VolumeAccuracy { get; set; }
        
        public int PriceAccuracy { get; set; }
        
        public decimal MinVolume { get; set; }
        
        public string DisplayName { get; set; }
        
        public string BaseAssetId { get; set; }
        
        public string QuoteAssetId { get; set; }
    }
}

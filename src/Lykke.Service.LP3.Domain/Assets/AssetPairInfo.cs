namespace Lykke.Service.LP3.Domain.Assets
{
    public class AssetPairInfo
    {
        public string AssetPairId { get; set; }
        
        public int VolumeAccuracy { get; set; }
        
        public int PriceAccuracy { get; set; }
        
        public decimal MinVolume { get; set; }
        
        public string DisplayName { get; set; }
    }
}

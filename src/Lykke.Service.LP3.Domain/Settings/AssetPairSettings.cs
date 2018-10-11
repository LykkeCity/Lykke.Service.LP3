namespace Lykke.Service.LP3.Domain.Settings
{
    public class AssetPairSettings
    {
        public string AssetPairId { get; set; }
        
        public bool IsReversed { get; set; }
        
        public string CrossInstrumentSource { get; set; }
        
        public string CrossInstrumentAssetPair { get; set; }
        
        public bool IsCrossInstrumentReversed { get; set; }
    }
}

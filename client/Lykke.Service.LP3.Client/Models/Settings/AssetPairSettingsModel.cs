namespace Lykke.Service.LP3.Client.Models.Settings
{
    public class AssetPairSettingsModel
    {
        public string AssetPairId { get; set; }
        
        public bool IsReversed { get; set; }
        
        public string CrossInstrumentSource { get; set; }
        
        public string CrossInstrumentAssetPair { get; set; }
        
        public bool IsCrossInstrumentReversed { get; set; }
    }
}

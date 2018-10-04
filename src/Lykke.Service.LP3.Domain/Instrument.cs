namespace Lykke.Service.LP3.Domain
{
    public class Instrument
    {
        public string AssetPair { get; set; }

        public string BaseAsset { get; set; }

        public string QuoteAsset { get; set; }

        public int PriceAccuracy { get; set; }

        public int VolumeAccuracy { get; set; }

        public decimal MinVolume { get; set; }
        
        public int BaseAssetAccuracy { get; set; }

        public int QuoteAssetAccuracy { get; set; }
    }
}

namespace Lykke.Service.LP3.Domain.CrossInstruments
{
    public class CrossInstrument
    {
        public string Exchange { get; }
        public string AssetPairId { get; }
        public string BaseAsset { get; }
        public string QuoteAsset { get; }

        public CrossInstrument(string exchange, string assetPairId, string baseAsset, string quoteAsset)
        {
            Exchange = exchange;
            AssetPairId = assetPairId;
            BaseAsset = baseAsset;
            QuoteAsset = quoteAsset;
        }
    }
}

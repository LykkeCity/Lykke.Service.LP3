using System;
using Common;
using Lykke.Service.LP3.Domain.Assets;

namespace Lykke.Service.LP3.Domain.Orders
{
    public class LimitOrder
    {
        public Guid Id { get; }

        public decimal Price { get; private set; }

        public decimal Volume { get; private set; }
        
        public TradeType TradeType { get; }
        
        public LimitOrderError Error { get; set; }
        
        public string ErrorMessage { get; set; }
        
        public string AssetPairId { get; set; }
        
        public LimitOrder(decimal price, decimal volume, TradeType tradeType)
            : this(Guid.NewGuid(), price, volume, tradeType)
        {
        }
        
        internal LimitOrder(Guid id, decimal price, decimal volume, TradeType tradeType)
        {
            Id = id;
            
            Price = price;
            Volume = volume;
            TradeType = tradeType;
        }

        public override string ToString()
        {
            return this.ToJson();
        }

        public void Round(AssetPairInfo assetPairInfo)
        {
            RoundVolume(assetPairInfo);
            RoundPrice(assetPairInfo);
        }

        private void RoundPrice(AssetPairInfo assetPairInfo)
        {
            var originalPrice = Price;
            Price = Price.TruncateDecimalPlaces(assetPairInfo.PriceAccuracy, toUpper: TradeType == TradeType.Sell);
            
            if (Price <= 0)
            {
                Error = LimitOrderError.InvalidPrice;
                ErrorMessage += $"Price is less or equal to zero. Original price: {originalPrice}, converted price: {Price}. ";
            }
        }

        private void RoundVolume(AssetPairInfo assetPairInfo)
        {
            var originalVolume = Volume;
            Volume = Math.Round(Math.Abs(Volume), assetPairInfo.VolumeAccuracy);

            if (Volume < assetPairInfo.MinVolume)
            {
                Error = LimitOrderError.TooSmallVolume;
                ErrorMessage +=
                    $"Minimal volume for {assetPairInfo.AssetPairId} is {assetPairInfo.MinVolume}. " +
                    $"Original volume: {originalVolume}, rounded volume: {Volume}. ";
            }
        }
    }
}

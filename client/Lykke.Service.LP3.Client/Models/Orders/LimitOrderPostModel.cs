using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.LP3.Client.Models.Orders
{
    public class LimitOrderPostModel
    {
        public string AssetPairId { get; set; }
        
        public decimal Price { get; set; }
        
        public decimal Volume { get; set; }
        
        public decimal Number { get; set; }
        
        [JsonConverter(typeof(StringEnumConverter))]
        public TradeType TradeType { get; set; }
    }
}

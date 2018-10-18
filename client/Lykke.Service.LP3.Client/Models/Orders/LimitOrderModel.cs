using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.LP3.Client.Models.Orders
{
    public class LimitOrderModel
    {
        public Guid Id { get; set; }

        public decimal Price { get; set; }

        public decimal Volume { get; set; }
        
        [JsonConverter(typeof(StringEnumConverter))]
        public TradeType TradeType { get; set; }
        
        [JsonConverter(typeof(StringEnumConverter))]
        public LimitOrderError Error { get; set; }
        
        public string ErrorMessage { get; set; }
        
        public string AssetPairId { get; set; }
    }
}

using System;

namespace Lykke.Service.LP3.Domain
{
    public class TickPrice
    {
        public string Source { get; set; }
        
        public string AssetPair { get; set; }
        
        public decimal Ask { get; set; }
        
        public decimal Bid { get; set; }
        
        public DateTime DateTime { get; set; }
    }
}

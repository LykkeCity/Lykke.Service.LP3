namespace Lykke.Service.LP3.Domain.TradingAlgorithm
{
    public class LevelState
    {
        public decimal VolumeBuy { get; set; }
        
        public decimal VolumeSell { get; set; }
        
        public decimal Inventory { get; set; }
        
        public decimal OppositeInventory { get; set; }
    }
}

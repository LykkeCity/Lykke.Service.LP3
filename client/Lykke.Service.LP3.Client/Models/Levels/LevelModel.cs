namespace Lykke.Service.LP3.Client.Models.Levels
{
    public class LevelModel
    {
        public decimal VolumeSell { get; set; }
        public decimal VolumeBuy { get; set; }
        public decimal Delta { get; set; }
        public decimal Reference { get; set; }
        public decimal OriginalVolume { get; set; }
        public string Name { get; set; }
        public decimal Inventory { get; set; }
        public decimal OppositeInventory { get; set; }
        public decimal Sell { get; set; }
        public decimal Buy { get; set; }
    }
}

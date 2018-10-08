namespace Lykke.Service.LP3.Domain.States
{
    public class LevelState
    {
        public string Name { get; }
        
        public decimal VolumeSell { get; set; }
        public decimal VolumeBuy { get; set; }
        //public decimal Reference { get; set; }
        public decimal Inventory { get; set; }
        public decimal OppositeInventory { get; set; }

        public LevelState(string name)
        {
            Name = name;
        }
    }
}

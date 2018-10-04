namespace Lykke.Service.LP3.Domain.Settings
{
    public class LevelSettings
    {
        public string Name { get; }
        
        public decimal Volume { get; }
        
        public decimal Delta { get; }

        public LevelSettings(string name, decimal volume, decimal delta)
        {
            Name = name;
            Volume = volume;
            Delta = delta;
        }
    }
}

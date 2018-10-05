using System;

namespace Lykke.Service.LP3.Domain.Settings
{
    public class SettingsChangedEventArgs : EventArgs
    {
        public LevelSettings ChangedLevel { get; set; }
        
        public LevelSettings AddedLevel { get; set; }
        
        public string NameOfDeletedLevel { get; set; }
    }
}

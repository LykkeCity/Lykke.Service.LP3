using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;

namespace Lykke.Service.LP3.AzureRepositories
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class LevelEntity : AzureTableEntity
    {
        public LevelEntity()
        {
            
        }

        public LevelEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }
        
        public string Name { get; set; }
        
        private decimal _delta;
        private decimal _volume;
        private decimal _reference;
        private decimal _inventory;
        private decimal _oppositeInventory;
        private decimal _volumeSell;
        private decimal _volumeBuy;

        public decimal Delta
        {
            get => _delta;
            set
            {
                _delta = value;
                MarkValueTypePropertyAsDirty(nameof(Delta));
            }
        }

        public decimal Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                MarkValueTypePropertyAsDirty(nameof(Volume));
            }
        }

        public decimal Reference
        {
            get => _reference;
            set
            {
                _reference = value;
                MarkValueTypePropertyAsDirty(nameof(Reference));
            }
        }

        public decimal Inventory
        {
            get => _inventory;
            set
            {
                _inventory = value;
                MarkValueTypePropertyAsDirty(nameof(Inventory));
            }
        }

        public decimal OppositeInventory
        {
            get => _oppositeInventory;
            set
            {
                _oppositeInventory = value;
                MarkValueTypePropertyAsDirty(nameof(OppositeInventory));
            }
        }

        public decimal VolumeSell
        {
            get => _volumeSell;
            set
            {
                _volumeSell = value;
                MarkValueTypePropertyAsDirty(nameof(VolumeSell));
            }
        }

        public decimal VolumeBuy
        {
            get => _volumeBuy;
            set
            {
                _volumeBuy = value;
                MarkValueTypePropertyAsDirty(nameof(VolumeBuy));
            }
        }
    }
}

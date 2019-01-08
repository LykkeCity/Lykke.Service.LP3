using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;

namespace Lykke.Service.LP3.AzureRepositories
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class OrderBookEntity : AzureTableEntity
    {
        public OrderBookEntity()
        {
        }

        public OrderBookEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }
        
        public string AssetPairId { get; set; }
        
        private bool _isEnabled;

        private decimal _volume;
        private int _count;
        private int _countInMarket;
        private decimal _delta;
        
        private decimal _initialPrice;
        private decimal _inventory;
        private decimal _oppositeInventory;
        private bool _isReverceBook = false;
        private int _volumeAccuracy = 0;


        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                MarkValueTypePropertyAsDirty(nameof(IsEnabled));
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

        public int Count
        {
            get => _count;
            set
            {
                _count = value;
                MarkValueTypePropertyAsDirty(nameof(Count));
            }
        }

        public int CountInMarket
        {
            get => _countInMarket;
            set
            {
                _countInMarket = value;
                MarkValueTypePropertyAsDirty(nameof(CountInMarket));
            }
        }

        public bool IsReverceBook
        {
            get => _isReverceBook;
            set
            {
                _isReverceBook = value;
                MarkValueTypePropertyAsDirty(nameof(IsReverceBook));
            }
        }

        public int VolumeAccuracy
        {
            get => _volumeAccuracy;
            set
            {
                _volumeAccuracy = value;
                MarkValueTypePropertyAsDirty(nameof(VolumeAccuracy));
            }
        }

        public decimal Delta
        {
            get => _delta;
            set
            {
                _delta = value;
                MarkValueTypePropertyAsDirty(nameof(Delta));
            }
        }

        public decimal InitialPrice
        {
            get => _initialPrice;
            set
            {
                _initialPrice = value;
                MarkValueTypePropertyAsDirty(nameof(InitialPrice));
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
    }
}

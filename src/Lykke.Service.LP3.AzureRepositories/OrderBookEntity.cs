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

using Lykke.AzureStorage.Tables;

namespace Lykke.Service.LP3.AzureRepositories
{
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

        private decimal _additionalOrdersDelta;
        private decimal _additionalOrdersVolume;
        private int _additionalOrdersCount;
        
        private decimal _levelDelta;
        private decimal _referencePrice;
        private decimal _inventory;
        private decimal _oppositeInventory;
        private decimal _levelVolumeSell;
        private decimal _levelVolumeBuy;
        private decimal _levelOriginalVolume;

        
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                MarkValueTypePropertyAsDirty(nameof(IsEnabled));
            }
        }
        

        public decimal AdditionalOrdersDelta
        {
            get => _additionalOrdersDelta;
            set
            {
                _additionalOrdersDelta = value;
                MarkValueTypePropertyAsDirty(nameof(AdditionalOrdersDelta));
            }
        }

        public decimal AdditionalOrdersVolume
        {
            get => _additionalOrdersVolume;
            set
            {
                _additionalOrdersVolume = value;
                MarkValueTypePropertyAsDirty(nameof(AdditionalOrdersVolume));
            }
        }

        public int AdditionalOrdersCount
        {
            get => _additionalOrdersCount;
            set
            {
                _additionalOrdersCount = value;
                MarkValueTypePropertyAsDirty(nameof(AdditionalOrdersCount));
            }
        }

        
        public decimal LevelDelta
        {
            get => _levelDelta;
            set
            {
                _levelDelta = value;
                MarkValueTypePropertyAsDirty(nameof(LevelDelta));
            }
        }

        public decimal ReferencePrice
        {
            get => _referencePrice;
            set
            {
                _referencePrice = value;
                MarkValueTypePropertyAsDirty(nameof(ReferencePrice));
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

        public decimal LevelVolumeSell
        {
            get => _levelVolumeSell;
            set
            {
                _levelVolumeSell = value;
                MarkValueTypePropertyAsDirty(nameof(LevelVolumeSell));
            }
        }

        public decimal LevelVolumeBuy
        {
            get => _levelVolumeBuy;
            set
            {
                _levelVolumeBuy = value;
                MarkValueTypePropertyAsDirty(nameof(LevelVolumeBuy));
            }
        }

        public decimal LevelOriginalVolume
        {
            get => _levelOriginalVolume;
            set
            {
                _levelOriginalVolume = value;
                MarkValueTypePropertyAsDirty(nameof(LevelOriginalVolume));
            }
        }
    }
}

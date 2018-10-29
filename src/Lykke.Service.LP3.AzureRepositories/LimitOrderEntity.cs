using System;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Service.LP3.Domain.Orders;

namespace Lykke.Service.LP3.AzureRepositories
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class LimitOrderEntity : AzureTableEntity
    {
        private string _externalId;
        private decimal _number;
        private decimal _price;
        private decimal _volume;
        private decimal _originalVolume;
        private TradeType _tradeType;
        private LimitOrderError _error;
        private string _errorMessage;
        private string _assetPairId;
        private Guid _id;

        public LimitOrderEntity()
        {
        }

        public LimitOrderEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        public Guid Id
        {
            get => _id;
            set
            {
                _id = value;
                MarkValueTypePropertyAsDirty(nameof(Id));
            }
        }

        public string ExternalId
        {
            get => _externalId;
            set
            {
                _externalId = value;
                MarkValueTypePropertyAsDirty(nameof(ExternalId));
            }
        }

        public decimal Number
        {
            get => _number;
            set
            {
                _number = value;
                MarkValueTypePropertyAsDirty(nameof(Number));
            }
        }

        public decimal Price
        {
            get => _price;
            set
            {
                _price = value;
                MarkValueTypePropertyAsDirty(nameof(Price));
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

        public decimal OriginalVolume
        {
            get => _originalVolume;
            set
            {
                _originalVolume = value;
                MarkValueTypePropertyAsDirty(nameof(OriginalVolume));
            }
        }

        public TradeType TradeType
        {
            get => _tradeType;
            set
            {
                _tradeType = value;
                MarkValueTypePropertyAsDirty(nameof(TradeType));
            }
        }

        public LimitOrderError Error
        {
            get => _error;
            set
            {
                _error = value;
                MarkValueTypePropertyAsDirty(nameof(Error));
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                MarkValueTypePropertyAsDirty(nameof(ErrorMessage));
            }
        }

        public string AssetPairId
        {
            get => _assetPairId;
            set
            {
                _assetPairId = value;
                MarkValueTypePropertyAsDirty(nameof(AssetPairId));
            }
        }
    }
}

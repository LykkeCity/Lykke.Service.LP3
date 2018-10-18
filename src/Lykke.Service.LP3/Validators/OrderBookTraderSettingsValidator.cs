using Common;
using FluentValidation;
using JetBrains.Annotations;
using Lykke.Service.LP3.Client.Models.Settings;

namespace Lykke.Service.LP3.Validators
{
    [UsedImplicitly]
    public class OrderBookTraderSettingsValidator : AbstractValidator<OrderBookTraderSettingsModel>
    {
        public OrderBookTraderSettingsValidator()
        {
            RuleFor(x => x.AssetPairId)
                .NotEmpty()
                .WithMessage("AssetPairId must be set")
                .Custom((name, context) =>
                {
                    if (!name.IsValidPartitionOrRowKey())
                    {
                        context.AddFailure("AssetPairId must be a valid row key");
                    }
                });
            
            RuleFor(x => x.LevelDelta)
                .GreaterThan(0)
                .WithMessage("LevelDelta must be positive");

            RuleFor(x => x.LevelOriginalVolume)
                .GreaterThan(0)
                .WithMessage("LevelOriginalVolume must be positive");
            
            RuleFor(x => x.AdditionalOrdersCount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("AdditionalOrdersCount must be non-negative");

            RuleFor(x => x.AdditionalOrdersDelta)
                .GreaterThan(0)
                .WithMessage("AdditionalOrdersDelta must be positive");

            RuleFor(x => x.AdditionalOrdersVolume)
                .GreaterThan(0)
                .WithMessage("AdditionalOrdersVolume must be positive");
            
            RuleFor(x => x.ReferencePrice)
                .GreaterThanOrEqualTo(0)
                .WithMessage("ReferencePrice must be non-negative");
        }
    }
}

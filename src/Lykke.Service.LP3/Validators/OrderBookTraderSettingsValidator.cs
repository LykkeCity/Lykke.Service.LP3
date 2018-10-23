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
            
            RuleFor(x => x.Delta)
                .GreaterThan(0)
                .WithMessage("Delta must be positive");

            RuleFor(x => x.Volume)
                .GreaterThan(0)
                .WithMessage("Volume must be positive");
            
            RuleFor(x => x.Count)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Count must be non-negative");

            RuleFor(x => x.InitialPrice)
                .GreaterThanOrEqualTo(0)
                .WithMessage("InitialPrice must be non-negative");
        }
    }
}

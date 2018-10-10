using Common;
using FluentValidation;
using JetBrains.Annotations;
using Lykke.Service.LP3.Client.Models.Settings;

namespace Lykke.Service.LP3.Validators
{
    [UsedImplicitly]
    public class LevelSettingsValidator : AbstractValidator<LevelSettingsModel>
    {
        public LevelSettingsValidator()
        {
            RuleFor(x => x.Name)
                .Custom((name, context) =>
                {
                    if (!name.IsValidPartitionOrRowKey())
                    {
                        context.AddFailure("Name must be a valid row key");
                    }
                });
            
            RuleFor(x => x.Delta)
                .GreaterThan(0)
                .WithMessage("Delta must be positive");

            RuleFor(x => x.Volume)
                .GreaterThan(0)
                .WithMessage("Volume must be positive");
        }
    }
}
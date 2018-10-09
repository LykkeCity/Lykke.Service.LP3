using FluentValidation;
using JetBrains.Annotations;
using Lykke.Service.LP3.Client.Models.Settings;

namespace Lykke.Service.LP3.Validators
{
    [UsedImplicitly]
    public class AdditionalVolumeSettingsValidator : AbstractValidator<AdditionalVolumeSettingsModel>
    {
        public AdditionalVolumeSettingsValidator()
        {
            RuleFor(x => x.Count)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Count must be non-negative");

            RuleFor(x => x.Delta)
                .GreaterThan(0)
                .WithMessage("Delta must be positive");

            RuleFor(x => x.Volume)
                .GreaterThan(0)
                .WithMessage("Volume must be positive");
        }
    }
}

using FluentValidation;
using JetBrains.Annotations;
using Lykke.Service.LP3.Client.Models.Settings;

namespace Lykke.Service.LP3.Validators
{
    [UsedImplicitly]
    public class BaseAssetPairSettingsModelValidator : AbstractValidator<BaseAssetPairSettingsModel>
    {
        public BaseAssetPairSettingsModelValidator()
        {
            RuleFor(x => x.AssetPairId)
                .NotEmpty()
                .WithMessage("AssetPairId must be set");
        }
    }
}

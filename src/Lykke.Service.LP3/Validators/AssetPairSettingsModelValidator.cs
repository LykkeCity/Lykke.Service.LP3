using FluentValidation;
using JetBrains.Annotations;
using Lykke.Service.LP3.Client.Models.Settings;

namespace Lykke.Service.LP3.Validators
{
    [UsedImplicitly]
    public class AssetPairSettingsModelValidator : AbstractValidator<AssetPairSettingsModel>
    {
        public AssetPairSettingsModelValidator()
        {
            RuleFor(x => x.AssetPairId)
                .NotEmpty()
                .WithMessage("AssetPairId must be set");

            RuleFor(x => x.CrossInstrumentSource)
                .NotEmpty()
                .WithMessage("CrossInstrumentSource must be set");

            RuleFor(x => x.CrossInstrumentAssetPair)
                .NotEmpty()
                .WithMessage("CrossInstrumentAssetPair must be set");
        }
    }
}

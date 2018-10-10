using FluentValidation;
using JetBrains.Annotations;
using Lykke.Service.LP3.Client.Models.CrossInstruments;

namespace Lykke.Service.LP3.Validators
{
    [UsedImplicitly]
    public class CrossInstrumentValidator : AbstractValidator<CrossInstrumentModel>
    {
        public CrossInstrumentValidator()
        {
            RuleFor(x => x.Exchange)
                .NotEmpty()
                .WithMessage("Exchange must be set");
            
            RuleFor(x => x.AssetPairId)
                .NotEmpty()
                .WithMessage("AssetPairId must be set");
            
            RuleFor(x => x.BaseAsset)
                .NotEmpty()
                .WithMessage("BaseAsset must be set");
            
            RuleFor(x => x.QuoteAsset)
                .NotEmpty()
                .WithMessage("QuoteAsset must be set");
        }
    }
}

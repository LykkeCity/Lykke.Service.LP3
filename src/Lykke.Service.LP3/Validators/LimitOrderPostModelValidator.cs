using FluentValidation;
using Lykke.Service.LP3.Client.Models.Orders;

namespace Lykke.Service.LP3.Validators
{
    public class LimitOrderPostModelValidator : AbstractValidator<LimitOrderPostModel>
    {
        public LimitOrderPostModelValidator()
        {
            RuleFor(x => x.AssetPairId)
                .NotEmpty()
                .WithMessage("AssetPairId must be set");
            
            RuleFor(x => x.Price)
                .GreaterThan(0)
                .WithMessage("Price must be positive");

            RuleFor(x => x.Volume)
                .GreaterThan(0)
                .WithMessage("Volume must be positive");
            
            RuleFor(x => x.TradeType)
                .IsInEnum()
                .WithMessage("TradeType must be valid enum value")
                .Custom((tradeType, context) =>
                {
                    if (tradeType != TradeType.Buy && tradeType != TradeType.Sell)
                    {
                        context.AddFailure("TradeType must be either Buy or Sell");
                    }
                });
        }
    }
}

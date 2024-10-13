using FluentValidation;
using SmartParking.SubscriptionTypes;


namespace SmartParking.Validations.SubscriptionTypes;

public class CreateUpdateSubscriptionTypeDtoValidator : AbstractValidator<CreateUpdateSubscriptionTypeDto>
{
    public CreateUpdateSubscriptionTypeDtoValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty()
            .Matches(@"^[a-zA-Z]*$")
            .WithMessage("Type must only contain letters , no special characters are allowed."); 
    }
}

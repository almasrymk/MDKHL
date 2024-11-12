using FluentValidation;
using SmartParking.SubscriptionTypes;


namespace SmartParking.Validations.SubscriptionTypes;

public class CreateUpdateSubscriptionTypeDtoValidator : AbstractValidator<CreateUpdateSubscriptionTypeDto>
{
    public CreateUpdateSubscriptionTypeDtoValidator()
    {
        RuleFor(x => x.Type)
        .NotEmpty()
        .Matches(@"^[\u0621-\u064A\u0660-\u0669a-zA-Z\s]*$")
        .WithMessage("Type must contain only Arabic and English letters, with no special characters allowed.");

    }
}

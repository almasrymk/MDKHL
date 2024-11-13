using FluentValidation;
using SmartParking.SubscriptionTypes;


namespace SmartParking.Validations.SubscriptionTypes;

public class CreateUpdateSubscriptionTypeDtoValidator : AbstractValidator<CreateUpdateSubscriptionTypeDto>
{
    public CreateUpdateSubscriptionTypeDtoValidator()
    {
		RuleFor(x => x.TypeEn)
		   .NotEmpty()
		   .MaximumLength(100)
		   .Matches(@"^[a-zA-Z][a-zA-Z0-9\s]*$")
		   .WithMessage("Name must start with a letter, accept only English letters and numbers, and cannot contain special characters such as ، / . \\ ,.");
		RuleFor(x => x.TypeAr)
			.NotEmpty()
			.MaximumLength(100)
			.Matches(@"^[\u0621-\u064A\s]+$")
			.WithMessage("Name must contain only Arabic letters and spaces, and cannot contain special characters such as ، / . \\ ,.");

	}
}

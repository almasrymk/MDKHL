using FluentValidation;
using SmartParking.Subscriptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Validations.Subscriptions;

public class CreateUpdateSubscriptionDtoValidator : AbstractValidator<CreateUpdateSubscriptionDto>
{
    public CreateUpdateSubscriptionDtoValidator()
    {
        RuleFor(x => x.ArabicName)
            .NotEmpty()
            .Matches(@"^[\p{IsArabic}][\p{IsArabic}\d،\.\,\/\\\s]*$")
            .WithMessage("The Arabic name should only contain Arabic letters, digits, punctuation marks, and spaces.")
            .MaximumLength(100)
            .WithMessage("Maximum Length Name must be only 100 Characters");

        RuleFor(x => x.EnglishName)
          .NotEmpty()
          .Matches(@"^[a-zA-Z][a-zA-Z0-9\s]*$")
          .WithMessage("Name must start with a letter, accept only English letters and numbers, and cannot contain special characters such as ، / . \\ ,.")
          .MaximumLength(100)
          .WithMessage("Maximum Length Name must be only 100 Characters");

        RuleFor(x => x.SubscriptionDescription)
            .NotEmpty();

        RuleFor(x => x.SubscriptionPrice)
           .NotEmpty()
           .GreaterThan(0);
    

        RuleFor(x => x.NumberOfUsers)
         .NotEmpty()
         .GreaterThan(0);

        RuleFor(x => x.NumbersOfGarages)
        .NotEmpty()
        .GreaterThan(0);

        RuleFor(x => x.NumberOfCameras)
        .NotEmpty()
        .GreaterThan(0);

        RuleFor(x => x.NumberOfMobileCustomers)
        .NotEmpty()
        .GreaterThan(0);

        RuleFor(x => x.SubscriptionTypeId)
        .NotEmpty()
        .GreaterThan(0);
    }

}

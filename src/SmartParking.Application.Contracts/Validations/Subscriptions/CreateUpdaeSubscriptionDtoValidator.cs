using FluentValidation;
using SmartParking.Subscriptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Validations.Subscriptions;

public class CreateUpdaeSubscriptionDtoValidator : AbstractValidator<CreateUpdateSubscriptionDto>
{
    public CreateUpdaeSubscriptionDtoValidator()
    {
        RuleFor(x => x.SubscriptionName)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Maximum Length Name must be only 100 Characters");

        RuleFor(x => x.SubscriptionDescription)
            .NotEmpty();

        RuleFor(x => x.SubscriptionPrice)
           .NotEmpty();

    

        RuleFor(x => x.ActivationDate)
         .NotEmpty();

        RuleFor(x => x.InActivationDate)
         .NotEmpty();

        RuleFor(x => x.NumberOfCustomers)
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

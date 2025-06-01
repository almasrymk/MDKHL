using FluentValidation;
using SmartParking.Localization;
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
            .Matches(ConstantValidations.ArabicName)
            .WithMessage(LocalizationHelper.GetLocalizedString("ArabicNameValidtion"))
            .MaximumLength(100)
            .WithMessage(LocalizationHelper.GetLocalizedString("MaxLengthNameValidation"));

        RuleFor(x => x.EnglishName)
          .NotEmpty()
          .Matches(ConstantValidations.EnglishName)
          .WithMessage(LocalizationHelper.GetLocalizedString("EnglishNameValidtion"))
          .MaximumLength(100)
          .WithMessage(LocalizationHelper.GetLocalizedString("MaxLengthNameValidation"));

        RuleFor(x => x.SubscriptionDescription)
            .NotEmpty();

        RuleFor(x => x.SubscriptionPrice)
           .NotEmpty()
           .GreaterThan(0);
    

        RuleFor(x => x.NumberOfUsers)
         .NotEmpty()
         .GreaterThan(0)
         .LessThan(10000)
         .WithMessage(LocalizationHelper.GetLocalizedString("LessThan9999Validation"));

        RuleFor(x => x.NumbersOfGarages)
        .NotEmpty()
        .GreaterThan(0)
        .LessThan(10000)
        .WithMessage(LocalizationHelper.GetLocalizedString("LessThan9999Validation"));

        RuleFor(x => x.NumberOfCameras)
        .NotEmpty()
        .GreaterThan(0)
        .LessThan(10000)
        .WithMessage( LocalizationHelper.GetLocalizedString("LessThan9999Validation"));


        RuleFor(x => x.NumberOfCustomers)
        .NotEmpty()
        .GreaterThan(0)
        .LessThan(10000)
        .WithMessage(LocalizationHelper.GetLocalizedString("LessThan9999Validation"));


        RuleFor(x => x.ParkingSlot)
       .NotEmpty()
       .GreaterThan(0)
       .LessThan(10000)
        .WithMessage(LocalizationHelper.GetLocalizedString("LessThan9999Validation"));
        

        RuleFor(x => x.SubscriptionTypeId)
        .NotEmpty()
        .GreaterThan(0);
    }

}

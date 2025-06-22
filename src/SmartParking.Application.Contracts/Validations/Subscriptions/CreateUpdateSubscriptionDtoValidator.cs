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
            .MaximumLength(100)
            .WithMessage(LocalizationHelper.GetLocalizedString("MaxLengthNameValidation"))
            .Matches(ConstantValidations.ArabicName)
            .WithMessage(LocalizationHelper.GetLocalizedString("ArabicNameValidtion"))
             .NotEmpty()
            .WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));

        RuleFor(x => x.EnglishName)
                 .MaximumLength(100)
          .WithMessage(LocalizationHelper.GetLocalizedString("MaxLengthNameValidation"))
          .Matches(ConstantValidations.EnglishName)
          .WithMessage(LocalizationHelper.GetLocalizedString("EnglishNameValidtion"))
               .NotEmpty()
          .WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));

        RuleFor(x => x.SubscriptionDescription)
            .NotEmpty();

        RuleFor(x => x.SubscriptionPrice)
           .NotEmpty()
           .GreaterThan(0);
    

        RuleFor(x => x.NumberOfUsers)
       .InclusiveBetween(0, to: 9999).WithMessage(LocalizationHelper.GetLocalizedString("RangeNumberValildation"))
             .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));

        RuleFor(x => x.NumbersOfGarages)
        .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
         .InclusiveBetween(0, to: 9999).WithMessage(LocalizationHelper.GetLocalizedString("RangeNumberValildation"));

        RuleFor(x => x.NumberOfCameras)
         .InclusiveBetween(0, to: 9999).WithMessage(LocalizationHelper.GetLocalizedString("RangeNumberValildation"))
             .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));


        RuleFor(x => x.NumberOfCustomers)
          .InclusiveBetween(0, to: 9999).WithMessage(LocalizationHelper.GetLocalizedString("RangeNumberValildation"))
             .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));


        RuleFor(x => x.ParkingSlot)
         .InclusiveBetween(0, to: 9999).WithMessage(LocalizationHelper.GetLocalizedString("RangeNumberValildation"))
             .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));


        RuleFor(x => x.SubscriptionTypeId) 
        .GreaterThan(0).WithMessage(LocalizationHelper.GetLocalizedString("GreaterZeroValidtion"))
        .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));
    }

}

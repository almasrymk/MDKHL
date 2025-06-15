using FluentValidation;
using SmartParking.Localization;
using SmartParking.OrganizationsGarages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Validations.Garages;

public class CreateUpdaeGarageDtoValidator : AbstractValidator<CreateUpdateGarageDto>
{
    public CreateUpdaeGarageDtoValidator()
    {
        RuleFor(x => x.NameEn)
            .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
            .MaximumLength(100)
            .Matches(ConstantValidations.EnglishName)
            .WithMessage(LocalizationHelper.GetLocalizedString("EnglishNameValidtion"));
        
        RuleFor(x => x.NameAr)
            .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
            .MaximumLength(100)
            .Matches(ConstantValidations.ArabicName).WithMessage(LocalizationHelper.GetLocalizedString("ArabicNameValidtion"));


        RuleFor(x => x.ClosingHours.ToString("HH:mm"))
            .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
           .Matches(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$").WithMessage(LocalizationHelper.GetLocalizedString("TimeOnlyValidation"));

        RuleFor(x=>x.MaxCapacity)
            .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
            .GreaterThan(0).WithMessage(LocalizationHelper.GetLocalizedString("GreaterZeroValidtion"))
            .LessThan(9999).WithMessage(LocalizationHelper.GetLocalizedString("LessThan9999Validation"));

        RuleFor(x => x.MaxSlotsForCustomers)
          .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
          .GreaterThan(0).WithMessage(LocalizationHelper.GetLocalizedString("GreaterZeroValidtion"))
          .LessThan(9999).WithMessage(LocalizationHelper.GetLocalizedString("LessThan9999Validation"));

        RuleFor(x => x.MaxSlotsForEmployess)
       .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
       .GreaterThan(0).WithMessage(LocalizationHelper.GetLocalizedString("GreaterZeroValidtion"))
       .LessThan(9999).WithMessage(LocalizationHelper.GetLocalizedString("LessThan9999Validation"));

        RuleFor(x => x.MaxSlotsForFree)
       .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
       .GreaterThan(0).WithMessage(LocalizationHelper.GetLocalizedString("GreaterZeroValidtion"))
       .LessThan(9999).WithMessage(LocalizationHelper.GetLocalizedString("LessThan9999Validation"));

        RuleFor(x => x.OrganizationId)
            .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
            .GreaterThan(0).WithMessage(LocalizationHelper.GetLocalizedString("GreaterZeroValidtion"));

        RuleFor(x => x.Status)
          .Must(value => Enum.IsDefined(typeof(Enums.Status), value))
         . WithMessage("The value must be between 0 and 2.");
    }
}

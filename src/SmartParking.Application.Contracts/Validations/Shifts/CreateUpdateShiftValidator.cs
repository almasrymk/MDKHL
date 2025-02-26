using FluentValidation;
using SmartParking.Localization;
using SmartParking.Shifts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Validations.Shifts;

public class CreateUpdateShiftValidator : AbstractValidator<CreateUpdateShiftDto>
{
    public CreateUpdateShiftValidator()
    {
        RuleFor(x => x.ArabicName)
        .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
        .Matches(@"^[\p{IsArabic}][\p{IsArabic}\d،\.\,\/\\\s]*$")
        .WithMessage(LocalizationHelper.GetLocalizedString("ArabicNameValidtion"));

        RuleFor(x => x.EnglishName)
         .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
            .Matches(@"^[a-zA-Z][a-zA-Z0-9/.,\\@#*]*(\s[a-zA-Z0-9/.,\\@#*]+)*$").WithMessage(LocalizationHelper.GetLocalizedString("EnglishNameValidtion"));

        RuleFor(x => x.Description)
         .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));

        RuleFor(x => x.StartDate)
            .Must(x => x != DateOnly.MinValue)
            .WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));

        RuleFor(x => x.EndDate)
            .Must(x => x != DateOnly.MinValue)
            .WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));

        RuleFor(x => x.StartDate)
            .LessThan(x => x.EndDate)
            .When(x => x.StartDate != x.EndDate) 
            .WithMessage(LocalizationHelper.GetLocalizedString("EndDateCannotBeBeforeStartDate"));

        RuleFor(x => x)
              .Must(x => !(x.IsFullDay == false && x.StartTime == TimeOnly.MinValue && x.EndTime == TimeOnly.MinValue))
              .WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation")+": "+LocalizationHelper.GetLocalizedString("StartTime")+","+ LocalizationHelper.GetLocalizedString("EndTime"));
    }
}

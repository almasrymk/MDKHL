using FluentValidation;
using SmartParking.Discounts;
using SmartParking.Localization;
using SmartParking.Shifts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Validations.Discounts;

public class DiscountCreateUpdateValidation : AbstractValidator<CreateUpdateDiscountDto>
{
    public DiscountCreateUpdateValidation()
    {
        RuleFor(x => x.ArabicName)
        .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
        .Matches(@"^[\p{IsArabic}][\p{IsArabic}\d،\.\,\/\\\s]*$")
        .WithMessage(LocalizationHelper.GetLocalizedString("ArabicNameValidtion"));

        RuleFor(x => x.EnglishName)
         .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
            .Matches(@"^[a-zA-Z][a-zA-Z0-9/.,\\@#*]*(\s[a-zA-Z0-9/.,\\@#*]+)*$").WithMessage(LocalizationHelper.GetLocalizedString("EnglishNameValidtion"));

        RuleFor(x => x.DiscountPercent)
            .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
            .InclusiveBetween(0, 100).WithMessage(LocalizationHelper.GetLocalizedString("PercentageValidtion"));
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
            .IsInEnum().WithMessage(LocalizationHelper.GetLocalizedString("StatusValidtion"));
            
    }
}

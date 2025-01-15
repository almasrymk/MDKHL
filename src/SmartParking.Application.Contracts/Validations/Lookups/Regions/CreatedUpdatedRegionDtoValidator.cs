using FluentValidation;
using SmartParking.Localization;
using SmartParking.Lookups.Region;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Validations.Lookups.Regions;

public class CreatedUpdatedRegionDtoValidator : AbstractValidator<CreateUpdateRegionDto>
{
    public CreatedUpdatedRegionDtoValidator()
    {
        RuleFor(x => x.ArabicName)
        .NotEmpty()
        .Matches(@"^[\p{IsArabic}][\p{IsArabic}\d،\.\,\/\\\s]*$")
        .WithMessage(LocalizationHelper.GetLocalizedString("ArabicNameValidtion"));

        RuleFor(x => x.EnglishName)
             .NotEmpty()
             .Matches(@"^[a-zA-Z][a-zA-Z0-9\s]*$")
              .WithMessage(LocalizationHelper.GetLocalizedString("EnglishNameValidtion"));

        RuleFor(x => x.CountryId)
            .NotEmpty()
            .GreaterThan(0);
    }
}

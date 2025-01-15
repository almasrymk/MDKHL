using FluentValidation;
using SmartParking.Localization;
using SmartParking.Lookups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Validations.Lookups.Countries;

public class CreatedUpdatedCountryDtoValidator : AbstractValidator<CreateUpdateCountryDto>
{
    public CreatedUpdatedCountryDtoValidator()
    {
        RuleFor(x => x.ArabicName)
         .NotEmpty()
         .Matches(@"^[\p{IsArabic}][\p{IsArabic}\d،\.\,\/\\\s]*$")
         .WithMessage(LocalizationHelper.GetLocalizedString("ArabicNameValidtion"));

        RuleFor(x => x.EnglishName)
             .NotEmpty()
             .Matches(@"^[a-zA-Z][a-zA-Z0-9\s]*$")
             .WithMessage(LocalizationHelper.GetLocalizedString("EnglishNameValidtion"));

        RuleFor(x => x.ArabicCapital)
            .NotEmpty()
            .Matches(@"^[\p{IsArabic}][\p{IsArabic}\d،\.\,\/\\\s]*$")
            .WithMessage("The Arabic name should only contain Arabic letters, digits, punctuation marks, and spaces.");

        RuleFor(x => x.EnglishCapital)
             .NotEmpty()
             .Matches(@"^[a-zA-Z][a-zA-Z0-9\s]*$")
             .WithMessage("Name must start with a letter, accept only English letters and numbers, and cannot contain special characters such as ، / . \\ ,.");
    }
}

using FluentValidation;
using SmartParking.Localization;
using SmartParking.Lookups.State;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Validations.Lookups.States;

public class CreatedUpdatedStateDtoValidator : AbstractValidator<CreateUpdateStateDto>
{
    public CreatedUpdatedStateDtoValidator()
    {
        RuleFor(x => x.ArabicName)
        .NotEmpty()
        .Matches(@"^[\p{IsArabic}][\p{IsArabic}\d،\.\,\/\\\s]*$")
         .WithMessage(LocalizationHelper.GetLocalizedString("ArabicNameValidtion"));


        RuleFor(x => x.EnglishName)
             .NotEmpty()
             .Matches(@"^[a-zA-Z][a-zA-Z0-9\s]*$")
             .WithMessage(LocalizationHelper.GetLocalizedString("EnglishNameValidtion"));

        RuleFor(x => x.RegionId)
            .NotEmpty()
            .GreaterThan(0);
    }
}

using FluentValidation;
using SmartParking.Localization;
using SmartParking.Lookups.PlateColor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Validations.Vehicles.VehiclesLookups
{
	public class CreateUpdatePlateColorDtoValidator :AbstractValidator<CreateUpdatePlateColorDto>
	{
        public CreateUpdatePlateColorDtoValidator()
        {

            RuleFor(x => x.ArabicName)
                .NotEmpty()
                .Matches(@"^[\p{IsArabic}][\p{IsArabic}\d،\.\,\/\\\s]*$")
                .WithMessage(LocalizationHelper.GetLocalizedString("ArabicNameValidtion"));

            RuleFor(x => x.EnglishName)
                .NotEmpty()
                .Matches(@"^[a-zA-Z][a-zA-Z0-9\s]*$")
                .WithMessage(LocalizationHelper.GetLocalizedString("EnglishNameValidtion"));
        }
    }
}

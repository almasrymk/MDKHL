using FluentValidation;
using SmartParking.Localization;
using SmartParking.Lookups.LicenceType;
using SmartParking.Lookups.VehicleModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Validations.Vehicles.VehiclesLookups
{
	public class CreateUpdateLicenceTypeDtoValidator : AbstractValidator<CreateUpdateLicenceTypeDto>
	{
        public CreateUpdateLicenceTypeDtoValidator()
        {
            RuleFor(x => x.ArabicName)
                .NotEmpty()
                .Matches(@"^[\p{IsArabic}][\p{IsArabic}\d،\.\,\/\\\s]*$")
                .WithMessage(LocalizationHelper.GetLocalizedString("ArabicNameValidtion"));

            RuleFor(x => x.EnglishName)
                .NotEmpty()
                .Matches(@"^[a-zA-Z][a-zA-Z0-9\s]*$")
                .WithMessage(LocalizationHelper.GetLocalizedString("EnglishNameValidtion"));

            RuleFor(x => x.PlateColorId)
                .NotEmpty()
                .GreaterThan(0);
		}
    }
}

using SmartParking.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using SmartParking.Localization;

namespace SmartParking.Validations.Vehicles;

public class CreatedUpdatedVehiclesDtoValidator:AbstractValidator<CreateUpdateVehicleDto>
{
        public CreatedUpdatedVehiclesDtoValidator()
        {
			RuleFor(x => x.VehicleModelId)
			.NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
            .GreaterThan(0);


			RuleFor(x => x.VehicleBrandId)
			.NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
            .GreaterThan(0);

			RuleFor(x => x.LicenceTypeId)
			.NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
            .GreaterThan(0);

			RuleFor(x => x.ModelYear)
            .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
            .Must(modelYear => int.TryParse(modelYear,out int year) && year >= 1980 && year <= DateTime.Now.Year)
			.WithMessage(LocalizationHelper.GetLocalizedString("MustBeGreaterThan")+' '+1980+ ' '+ LocalizationHelper.GetLocalizedString("MustBeSmallerThan")+' '+DateTime.Now.Year);

            RuleFor(x => x.ArPlateLetters)
            .NotNull().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
            .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
            .Matches(@"^[\p{IsArabic}\d\s]+$").WithMessage(LocalizationHelper.GetLocalizedString("OnlyArabicLettersAllowed"))
            .Length(1, 3).WithMessage(LocalizationHelper.GetLocalizedString("RangeValidation") + "1-3")
            .When(x => !string.IsNullOrWhiteSpace(x.ArPlateLetters)).WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation")); 

        RuleFor(x => x.EnPlateLetters)
             .NotNull().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
             .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
             .Matches(@"^[A-Za-z\d\s]+$").WithMessage(LocalizationHelper.GetLocalizedString("OnlyEnglishLettersAllowed"))
            .Length(1, 3).WithMessage(LocalizationHelper.GetLocalizedString("RangeValidation") + "1-3")
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));

        RuleFor(x => x.ArPlateNumber)
             .NotNull().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
             .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
             .Matches(@"^[0-9\u0660-\u0669\s]+$").WithMessage(LocalizationHelper.GetLocalizedString("OnlyArabicNumberAllowed"))
             .Length(1, 4).WithMessage(LocalizationHelper.GetLocalizedString("RangeValidation") + " 1-4")
             .When(x => !string.IsNullOrWhiteSpace(x.ArPlateNumber)).WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));  


        RuleFor(x => x.EnPlateNumber)
            .NotNull().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
             .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
            .Matches(@"^[0-9\s]+$").WithMessage(LocalizationHelper.GetLocalizedString("OnlyEnglishNumberAllowed"))
            .Length(1, 4).WithMessage(LocalizationHelper.GetLocalizedString("RangeValidation") + " 1-4")
            .When(x => !string.IsNullOrWhiteSpace(x.EnPlateNumber)).WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));


        RuleFor(x => x.BelongToCompany)
              .Must(b => b != null)
              .WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));


        RuleFor(x => x.CompanyName)
            .NotEmpty()
            .When(x => x.BelongToCompany)
            .WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));





    }
}

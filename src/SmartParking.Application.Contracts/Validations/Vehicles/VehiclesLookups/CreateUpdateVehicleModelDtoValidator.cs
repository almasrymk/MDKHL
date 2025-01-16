using FluentValidation;
using SmartParking.Localization;
using SmartParking.Lookups.VehicleModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Validations.Vehicles.VehiclesLookups;

public class CreateUpdateVehicleModelDtoValidator : AbstractValidator<CreateUpdateVehicleModelDto>
{
    public CreateUpdateVehicleModelDtoValidator()
    {
        RuleFor(x=>x.ArabicName)
          .NotEmpty()
          .Matches(@"^[\p{IsArabic}][\p{IsArabic}\d،\.\,\/\\\s]*$")
          .WithMessage(LocalizationHelper.GetLocalizedString("ArabicNameValidtion"));

        RuleFor(x=>x.EnglishName)
            .NotEmpty()
            .Matches(@"^[a-zA-Z][a-zA-Z0-9\s]*$")
             .WithMessage(LocalizationHelper.GetLocalizedString("EnglishNameValidtion"));

        RuleFor(x=>x.VehiclesBrandId)
            .NotEmpty()
            .GreaterThan(0);
    }
}

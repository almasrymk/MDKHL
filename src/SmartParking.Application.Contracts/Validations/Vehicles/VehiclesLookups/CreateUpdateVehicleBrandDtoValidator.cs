using FluentValidation;
using SmartParking.Lookups.VehicleBrand;
using SmartParking.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Validations.Vehicles.VehiclesLookups;

public class CreateUpdateVehicleBrandDtoValidator : AbstractValidator<CreateUpdateVehicleBrandDto>
{
    public CreateUpdateVehicleBrandDtoValidator()
    {
		RuleFor(x => x.ArabicName).NotEmpty();
		RuleFor(x => x.EnglishName).NotEmpty();

	}
}

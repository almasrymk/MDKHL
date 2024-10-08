using SmartParking.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace SmartParking.Validations.Vehicles;

public class CreatedUpdatedVehiclesDtoValidator:AbstractValidator<CreateUpdateVehicleDto>
{
        public CreatedUpdatedVehiclesDtoValidator()
        {
		RuleFor(x => x.VehicleModelId).NotEmpty();
		RuleFor(x => x.VehicleBrandId).NotEmpty();
		RuleFor(x => x.PlateColorId).NotEmpty();
		RuleFor(x => x.LicenceTypeId).NotEmpty();
		RuleFor(x => x.ModelYear).NotEmpty();
		RuleFor(x => x.PlateNumber).NotEmpty();
		// Check that BelongToCompany is not null and if it's true, ensure CompanyName is provided
		RuleFor(x => x.BelongToCompany)
			.Must(b => b != null)
			.WithMessage("'Belong To Company' must not be empty.");

		// Add a rule that company name must be provided if BelongToCompany is true
		RuleFor(x => x.CompanyName)
			.NotEmpty()
			.When(x => x.BelongToCompany)
			.WithMessage("Company Name must be provided when Belong To Company is true.");


	}
}

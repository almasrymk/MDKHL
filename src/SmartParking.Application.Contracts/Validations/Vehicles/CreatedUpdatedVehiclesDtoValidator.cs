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
			RuleFor(x => x.VehicleModelId)
			.NotEmpty()
			.GreaterThan(0);
			RuleFor(x => x.VehicleBrandId)
			.NotEmpty()
			.GreaterThan(0);

			RuleFor(x => x.PlateColorId)
			.NotEmpty()
			.GreaterThan(0);

			RuleFor(x => x.LicenceTypeId)
			.NotEmpty()
            .GreaterThan(0);

			RuleFor(x => x.ModelYear).NotEmpty()
			.Must(modelYear => int.TryParse(modelYear,out int year) && year >= 1980)
			.WithMessage("The Model Year must be greater than or equal  1980.");

			RuleFor(x => x.PlateNumber).NotEmpty()
				.Matches(@"^[\p{IsArabic}\d\s]+$")
				.WithMessage("The Plate Number should only contain Arabic letters, numbers, and spaces.");

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

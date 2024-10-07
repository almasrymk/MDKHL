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
			// Check that BelongToCompany is not null and if it's true, ensure CompanyName is provided
			RuleFor(x => x.BelongToCompany)
				.Must(b => b != null)
				.WithMessage("'Belong To Company' must not be empty.");


			RuleFor(x => x.CompanyName)
				.Null()
				.When(x => (x.BelongToCompany == false))
				.WithMessage("CompanyName must be null or empty when the vehicle does not belong to a company.");
			RuleFor(x => x.CompanyName)
				.Empty()
				.When(x => (x.BelongToCompany == false))
				.WithMessage("CompanyName must be null or empty when the vehicle does not belong to a company.");
			// Add a rule that company name must be provided if BelongToCompany is true
			RuleFor(x => x.CompanyName)
				.NotEmpty()
				.When(x => (x.BelongToCompany == true))
				.WithMessage("Company Name must be provided when Belong To Company is true.");
    }
}

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

			RuleFor(x => x.LicenceTypeId)
			.NotEmpty()
            .GreaterThan(0);

			RuleFor(x => x.ModelYear).NotEmpty()
			.Must(modelYear => int.TryParse(modelYear,out int year) && year >= 1980)
			.WithMessage("The Model Year must be greater than or equal  1980.");

             RuleFor(x => x.ArPlateLetters).NotEmpty()
            .Matches(@"^[\p{IsArabic}\d\s]+$")
            .WithMessage("The Plate Letter should only contain Arabic letters")
            .When(x => x.ArPlateLetters.Length != 0)
            .MinimumLength(1)
            .When(x => x.ArPlateLetters.Length != 0)
            .MaximumLength(3)
            .WithMessage("The Plate Arabic Letter must be between 1 and 3 characters long.");


             RuleFor(x => x.EnPlateLetters).NotEmpty()
            .Matches(@"^[A-Za-z\d\s]+$")
            .WithMessage("The Plate Letter should only contain English letters")
            .When(x => x.EnPlateLetters.Length != 0)
            .MinimumLength(1)
            .When(x => x.EnPlateLetters.Length != 0)
            .MaximumLength(3)
            .WithMessage("The Plate English Letter must be between 1 and 3 characters long.");

             RuleFor(x => x.ArPlateNumber)
            .NotEmpty()
            .WithMessage("Arabic plate number can't be empty")          
            .Matches(@"^[0-9\u0660-\u0669\s]+$")
            .WithMessage("The Plate Number should only contain Arabic numerals.")
            .When(x => x.ArPlateNumber.Length != 0)
            .MinimumLength(1)
            .When(x => x.ArPlateNumber.Length != 0)
            .MaximumLength(4)
            .WithMessage("The Plate Number must be between 1 and 4 Arabic numerals long.");

             RuleFor(x => x.EnPlateNumber)
            .NotEmpty()
            .WithMessage("English plate number can't be empty")
            .Matches(@"^[0-9\s]+$")
            .WithMessage("The Plate Number should only contain English numerals.")
            .When(x => x.EnPlateNumber.Length != 0)
            .MinimumLength(1)
            .When(x => x.EnPlateNumber.Length != 0)
            .MaximumLength(4)
            .WithMessage("The Plate Number must be between 1 and 4 English numerals long.");

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

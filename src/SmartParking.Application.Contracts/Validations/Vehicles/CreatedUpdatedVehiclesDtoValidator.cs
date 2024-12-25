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

        RuleFor(x => x.ArPlateLetters)
  .NotNull().WithMessage("The Plate Letter cannot be null.")
  .NotEmpty().WithMessage("The Plate Letter cannot be empty.")
  .Matches(@"^[\p{IsArabic}\d\s]+$").WithMessage("The Plate Letter should only contain Arabic letters, digits, or spaces.")
  .Length(1, 3).WithMessage("The Plate Arabic Letter must be between 1 and 3 characters long.")
  .When(x => !string.IsNullOrWhiteSpace(x.ArPlateLetters)); 

        RuleFor(x => x.EnPlateLetters)
  .NotNull().WithMessage("The Plate Letter cannot be null.")
  .NotEmpty().WithMessage("The Plate Letter cannot be empty.")
  .Matches(@"^[A-Za-z\d\s]+$").WithMessage("The Plate Letter should only contain English letters, digits, or spaces.")
  .Length(1, 3).WithMessage("The Plate English Letter must be between 1 and 3 characters long.")
  .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("The Plate Letter cannot be just whitespace.");

        RuleFor(x => x.ArPlateNumber)
    .NotNull().WithMessage("Arabic plate number can't be null.") 
    .NotEmpty().WithMessage("Arabic plate number can't be empty")  
    .Matches(@"^[0-9\u0660-\u0669\s]+$").WithMessage("The Plate Number should only contain Arabic numerals.")
    .Length(1, 4).WithMessage("The Plate Number must be between 1 and 4 Arabic numerals long.")
    .When(x => !string.IsNullOrWhiteSpace(x.ArPlateNumber));  


        RuleFor(x => x.EnPlateNumber)
            .NotNull().WithMessage("English plate number can't be null.")  
            .NotEmpty().WithMessage("English plate number can't be empty") 
            .Matches(@"^[0-9\s]+$").WithMessage("The Plate Number should only contain English numerals.")
            .Length(1, 4).WithMessage("The Plate Number must be between 1 and 4 English numerals long.")
            .When(x => !string.IsNullOrWhiteSpace(x.EnPlateNumber));  


        RuleFor(x => x.BelongToCompany)
				.Must(b => b != null)
				.WithMessage("'Belong To Company' must not be empty.");

			RuleFor(x => x.CompanyName)
				.NotEmpty()
				.When(x => x.BelongToCompany)
				.WithMessage("Company Name must be provided when Belong To Company is true.");




       
    }
}

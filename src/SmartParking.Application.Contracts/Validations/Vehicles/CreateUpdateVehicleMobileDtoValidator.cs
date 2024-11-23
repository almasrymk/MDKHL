using FluentValidation;
using SmartParking.Mobile.Vehicles;
using SmartParking.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Validations.Vehicles;

public class CreateUpdateVehicleMobileDtoValidator : AbstractValidator<CreateUpdateVehicleMobileDto>
{
    public CreateUpdateVehicleMobileDtoValidator()
    {
        RuleFor(x => x.BrandId)
        .NotEmpty()
        .GreaterThan(0);
        RuleFor(x => x.ModelId)
        .NotEmpty()
        .GreaterThan(0);

        RuleFor(x => x.plateType)
        .NotEmpty();

        RuleFor(x => x.Year).NotEmpty()
        .Must(modelYear => int.TryParse(modelYear, out int year) && year >= 1980)
        .WithMessage("The Model Year must be greater than or equal  1980.");

        RuleFor(x => x.ArPlateLetters)
            .NotEmpty()
            .Matches(@"^[\p{IsArabic}\d\s]+$")
            .WithMessage("The Plate Letter should only contain Arabic letters")
            .When(x => x.ArPlateLetters.Length != 0)
            .MinimumLength(1)
            .When(x => x.ArPlateLetters.Length != 0)
            .MaximumLength(3)
            .WithMessage("The Plate Arabic Letter must be between 1 and 3 characters long.");


        RuleFor(x => x.EnPlateLetters)
            .NotEmpty()
            .Matches(@"^[A-Za-z\d\s]+$")
            .WithMessage("The Plate Letter should only contain English letters")
            .When(x => x.EnPlateLetters.Length != 0)
            .MinimumLength(1)
            .When(x => x.EnPlateLetters.Length != 0)
            .MaximumLength(3)
            .WithMessage("The Plate English Letter must be between 1 and 3 characters long.");

        RuleFor(x => x.ArPlateNumber)
            .NotEmpty()
            .Matches(@"^[\u0660-\u0669\s]+$")
            .WithMessage("The Plate Number should only contain Arabic numerals.")
            .When(x => x.ArPlateNumber.Length != 0)
            .MinimumLength(1)
            .When(x => x.ArPlateNumber.Length != 0)
            .MaximumLength(4)
            .WithMessage("The Plate Number must be between 1 and 4 Arabic numerals long.");

        RuleFor(x => x.EnPlateNumber)
            .NotEmpty()
            .Matches(@"^[0-9\s]+$")
            .WithMessage("The Plate Number should only contain English numerals.")
            .When(x => x.EnPlateNumber.Length != 0)
            .MinimumLength(1)
            .When(x => x.EnPlateNumber.Length != 0)
            .MaximumLength(4)
            .WithMessage("The Plate Number must be between 1 and 4 English numerals long.");



    }
}

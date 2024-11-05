using FluentValidation;
using SmartParking.OrganizationsGarages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Validations.Garages;

public class CreateUpdaeGarageDtoValidator : AbstractValidator<CreateUpdateGarageDto>
{
    public CreateUpdaeGarageDtoValidator()
    {
        RuleFor(x => x.NameEn)
            .NotEmpty()
            .MaximumLength(100)
            .Matches(@"^[a-zA-Z][a-zA-Z0-9\s]*$")
            .WithMessage("Name must start with a letter, accept only English letters and numbers, and cannot contain special characters such as ، / . \\ ,.");
        RuleFor(x => x.NameAr)
            .NotEmpty()
            .MaximumLength(100)
            .Matches(@"^[\u0621-\u064A\s]+$")
            .WithMessage("Name must contain only Arabic letters and spaces, and cannot contain special characters such as ، / . \\ ,.");

        RuleFor(x => x.ClosingHours.ToString("HH:mm"))
           .Matches(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$")
           .WithMessage("Time must be in the format HH:MM.").NotEmpty();
        RuleFor(x=>x.MaxCapacity).NotEmpty().GreaterThan(0).LessThan(9999);
        RuleFor(x => x.OrganizationId).NotEmpty().GreaterThan(0);
        RuleFor(x => x.Status)         
            .Must(value => Enum.IsDefined(typeof(Enums.Status), value))
         .WithMessage("The value must be between 0 and 2.");
    }
}

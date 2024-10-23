using FluentValidation;
using SmartParking.Cutomers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Validations.CustomerValidations;

public class RegisterCustomerDtoValidator : AbstractValidator<CreateUpdateCustomerDto>
{
    public RegisterCustomerDtoValidator()
    {
        RuleFor(x => x.ArabicName)
    .NotEmpty()
    .Matches(@"^([\u0600-\u06FF]+\s+){2}[\u0600-\u06FF]+$")
    .WithMessage("ArabicName must contain at least three Arabic words.")
    .MinimumLength(10)
    .WithMessage("ArabicName must be at least 10 characters long.");

        RuleFor(x => x.FullName)
            .NotEmpty()
            .Matches(@"^([a-zA-Z]+\s+){2}[a-zA-Z]+$")
            .WithMessage("FullName must contain at least three English words.")
            .MinimumLength(10)
            .WithMessage("FullName must be at least 10 characters long.");

        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required").MinimumLength(8).WithMessage("Password is minimum length is 8").Matches(x=>x.RepeatPassword).WithMessage("Password must match repeat password");
        RuleFor(x => x.RepeatPassword).NotEmpty().WithMessage("Password is required").MinimumLength(8).WithMessage("Password is minimum length is 8").Matches(x=>x.Password).WithMessage("RepeatPassword must match Password");
        RuleFor(x => x.MobileNumber).NotEmpty().Matches(@"^\d{10,15}$");
        RuleFor(x => x.NationalId).Length(10);
        RuleFor(x => x.isActive)
            .Must(value => value == true || value == false)
            .WithMessage("The IsActive field must be either true (1) or false (0).");
        RuleFor(x=>x.IdentificationType).Must(value => Enum.IsDefined(typeof(Enums.IdentificationTypes), value))
             .WithMessage("The value must be NationalID(1) or Passport(0).");
    }
}

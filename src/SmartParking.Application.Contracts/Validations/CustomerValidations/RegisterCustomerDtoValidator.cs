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
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required").MinimumLength(8).WithMessage("Password is minimum length is 8").Matches(x=>x.RepeatPassword).WithMessage("Password must match repeat password");
        RuleFor(x => x.RepeatPassword).NotEmpty().WithMessage("Password is required").MinimumLength(8).WithMessage("Password is minimum length is 8").Matches(x=>x.Password).WithMessage("RepeatPassword must match Password");
        RuleFor(x => x.MobileNumber).Matches(@"^\d{10,15}$");
        RuleFor(x => x.NationalId).Length(10);
    }
}

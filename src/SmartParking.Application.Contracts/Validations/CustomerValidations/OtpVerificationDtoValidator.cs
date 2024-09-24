using FluentValidation;
using SmartParking.Cutomers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Validations.CustomerValidations;

public class OtpVerificationDtoValidator : AbstractValidator<OtpVerificationDto>
{
    public OtpVerificationDtoValidator()
    {
        RuleFor(x => x.MobileNumber).Matches(@"^\d{10,15}$");
        RuleFor(x => x.OtpCode).NotEmpty();
    }
}
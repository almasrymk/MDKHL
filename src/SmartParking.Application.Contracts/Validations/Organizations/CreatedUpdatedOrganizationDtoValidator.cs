using FluentValidation;
using SmartParking.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Validations.Organizations;

public class CreatedUpdatedOrganizationDtoValidator : AbstractValidator<CreateUpdateOrganiztionDto>
{
    public CreatedUpdatedOrganizationDtoValidator()
    {
        //    RuleFor(x=>x.UnifiedLicenseNum).Length(9).NotEmpty().Matches();
        RuleFor(x => x.PhoneNum).Matches(@"^\d+$").WithMessage("The field must contain only numbers.").MinimumLength(9).WithMessage("Phone Number Can't be less than 9 digits").MaximumLength(10).WithMessage("Phone Number Can't be less than 9 digits");  
    }
}

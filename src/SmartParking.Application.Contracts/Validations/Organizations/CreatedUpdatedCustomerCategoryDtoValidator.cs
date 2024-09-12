using FluentValidation;
using SmartParking.CustomerCategories;
using SmartParking.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Validations.Organizations;

public class CreatedUpdatedCustomerCategoryDtoValidator : AbstractValidator<CreateUpdateCutomerCategoryDto>
{
    public CreatedUpdatedCustomerCategoryDtoValidator()
    {

        RuleFor(x=>x.Name).NotEmpty().WithMessage("Name can't be empty").MinimumLength(2).WithMessage("It can't be less than two digits").MaximumLength(30).WithMessage("It can't be more than 30 digits");
        RuleFor(x=>x.Description).NotEmpty().WithMessage("Name can't be empty").MinimumLength(2).WithMessage("It can't be less than two digits").MaximumLength(500).WithMessage("It can't be more than 500 digits");
    }
}

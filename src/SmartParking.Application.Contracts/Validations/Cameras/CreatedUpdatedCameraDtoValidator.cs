using FluentValidation;
using SmartParking.Cameras;
using SmartParking.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Validations.Cameras;

public class CreatedUpdatedCameraDtoValidator : AbstractValidator<CreateUpdateCameraDto>
{
    public CreatedUpdatedCameraDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        




    }
}

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
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Model).NotEmpty();
        RuleFor(x => x.MainType).NotEmpty();
        RuleFor(x => x.BrandName).NotEmpty();
        RuleFor(x => x.Code).NotEmpty();
        RuleFor(x => x.CameraIP).NotEmpty();
        RuleFor(x => x.MacAddress).NotEmpty();
        RuleFor(x => x.Type).NotEmpty();
        RuleFor(x => x.GarageId).NotEmpty();
        
        RuleFor(x => x.TenantId).NotEmpty();




    }
}

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
        RuleFor(x => x.Name)
            .NotEmpty();

        RuleFor(x => x.Status)
            .IsInEnum()
            .NotEmpty();

        RuleFor(x => x.CameraType)
            .IsInEnum()
            .NotEmpty();

        RuleFor(x => x.BrandName)
            .NotEmpty();

        RuleFor(x => x.Model)
           .NotEmpty(); 
        
        RuleFor(x => x.MainType)
           .NotEmpty();     
        
        RuleFor(x => x.CameraIP)
           .NotEmpty();        

        RuleFor(x => x.MacAddress)
           .NotEmpty();


    }
}

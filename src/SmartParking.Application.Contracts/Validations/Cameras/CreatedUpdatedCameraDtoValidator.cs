using FluentValidation;
using Microsoft.Extensions.Localization;
using SmartParking.Cameras;
using SmartParking.Localization;
using SmartParking.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Validations.Cameras;

public class CreatedUpdatedCameraDtoValidator : AbstractValidator<CreateUpdateCameraDto>
{
    private readonly IStringLocalizer<SmartParkingResource> _stringLocalizer;

    public CreatedUpdatedCameraDtoValidator(IStringLocalizer<SmartParkingResource> stringLocalizer)
    {
        _stringLocalizer = stringLocalizer;

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(_stringLocalizer["Should Not be empty."]);

        RuleFor(x => x.Status)
            .IsInEnum();
            

        RuleFor(x => x.CameraType)
            .IsInEnum();


        RuleFor(x => x.BrandNameId)
            .NotEmpty();


        RuleFor(x => x.ModelId)
            .NotEmpty();

        RuleFor(x => x.MainTypeId)
            .NotEmpty();
          


        RuleFor(x => x.CameraIP)
           .NotEmpty()
            .WithMessage(_stringLocalizer["Should Not be empty."]);


        RuleFor(x => x.MacAddress)
           .NotEmpty()
            .WithMessage(_stringLocalizer["Should Not be empty."]);



    }
}

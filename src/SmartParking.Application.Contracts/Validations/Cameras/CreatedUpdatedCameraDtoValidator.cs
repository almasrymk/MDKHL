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

    public CreatedUpdatedCameraDtoValidator()
    {

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));

        RuleFor(x => x.Status)
            .IsInEnum();
            
        RuleFor(x => x.CameraType)
            .IsInEnum();

        RuleFor(x => x.BrandNameId)
            .NotEmpty()
            .WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));

        RuleFor(x => x.ModelId)
            .NotEmpty()
            .WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));

        RuleFor(x => x.MainTypeId)
            .NotEmpty()
            .WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));

        RuleFor(x => x.CameraIP)
           .NotEmpty()
           .WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));

        RuleFor(x => x.MacAddress)
           .NotEmpty()
            .WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));
        RuleFor(x => x.UserName)
           .NotEmpty()
            .WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));
        RuleFor(x => x.Password)
           .NotEmpty()
            .WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));

    }
}

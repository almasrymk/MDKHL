using FluentValidation;
using Microsoft.Extensions.Localization;
using SmartParking.Cameras;
using SmartParking.Localization;
using SmartParking.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SmartParking.Validations.Cameras;

public class CreatedUpdatedCameraDtoValidator : AbstractValidator<CreateUpdateCameraDto>
{
    public CreatedUpdatedCameraDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(LocalizationHelper.GetLocalizedString("RequiredValidation"));

        RuleFor(x => x.Status)
            .IsInEnum();

        RuleFor(x => x.CameraType)
            .IsInEnum();

        RuleFor(x => x.BrandNameId)
            .NotEmpty()
            .WithMessage(LocalizationHelper.GetLocalizedString("RequiredValidation"));

        RuleFor(x => x.ModelId)
            .NotEmpty()
            .WithMessage(LocalizationHelper.GetLocalizedString("RequiredValidation"));

        RuleFor(x => x.MainTypeId)
            .NotEmpty()
            .WithMessage(LocalizationHelper.GetLocalizedString("RequiredValidation"));

        RuleFor(x => x.CameraIP)
            .NotEmpty()
            .WithMessage(LocalizationHelper.GetLocalizedString("RequiredValidation"))
            .Matches(@"^(25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)){3}$")
            .WithMessage(LocalizationHelper.GetLocalizedString("InvalidIPAddressValidation"));

        RuleFor(x => x.MacAddress)
            .NotEmpty()
            .WithMessage(LocalizationHelper.GetLocalizedString("RequiredValidation"))
            .Matches(@"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$")
            .WithMessage(LocalizationHelper.GetLocalizedString("InvalidMacAddressValidation"));

        RuleFor(x => x.UserName)
            .NotEmpty()
            .WithMessage(LocalizationHelper.GetLocalizedString("RequiredValidation"));

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage(LocalizationHelper.GetLocalizedString("RequiredValidation"));

        RuleFor(x => x.DeviceId)
            .NotEmpty()
            .WithMessage(LocalizationHelper.GetLocalizedString("RequiredValidation"));
    }
}

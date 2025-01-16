using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using SmartParking.Localization;
using SmartParking.Lookups.CameraBrands;
using SmartParking.Lookups.CameraMainType;

namespace SmartParking.Validations.Lookups.Cameras
{
    internal class CreatedUpdatedCameraMainTypesDtoValidator : AbstractValidator<CreateUpdateCameraMainTypeDto>
    {
        public CreatedUpdatedCameraMainTypesDtoValidator()
        {
            RuleFor(x => x.ArabicName)
                .NotEmpty()
                .Matches(@"^[\p{IsArabic}][\p{IsArabic}\d،\.\,\/\\\s]*$")
                .WithMessage(LocalizationHelper.GetLocalizedString("ArabicNameValidtion"));

            RuleFor(x => x.EnglishName)
                 .NotEmpty()
                 .Matches(@"^[a-zA-Z][a-zA-Z0-9\s!@#$%^&*()_+\-=\[\]{};':\\|,.<>\/?]* $")
                 .WithMessage(LocalizationHelper.GetLocalizedString("EnglishNameValidtion"));

            RuleFor(x => x.CameraBrandId)
            .NotEmpty()
            .GreaterThan(0);
        }
    }

}
    

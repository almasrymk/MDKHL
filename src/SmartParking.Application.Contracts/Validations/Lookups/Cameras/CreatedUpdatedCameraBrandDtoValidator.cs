using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using SmartParking.Cameras;
using SmartParking.Localization;
using SmartParking.Lookups;
using SmartParking.Lookups.CameraBrands;

namespace SmartParking.Validations.Lookups.Cameras
{
    internal class CreatedUpdatedCameraBrandDtoValidator : AbstractValidator<CreateUpdateCameraBrandDto>
    {
        public CreatedUpdatedCameraBrandDtoValidator()
        {
            RuleFor(x => x.ArabicName)
                .NotEmpty()
                .Matches(@"^[\p{IsArabic}][\p{IsArabic}\d،\.\,\/\\\s]*$")
                .WithMessage(LocalizationHelper.GetLocalizedString("ArabicNameValidtion"));

            RuleFor(x => x.EnglishName)
                 .NotEmpty()
                 .Matches(@"^[a-zA-Z][a-zA-Z0-9\s]*$")
                 .WithMessage(LocalizationHelper.GetLocalizedString("EnglishNameValidtion"));
        }
    }

}

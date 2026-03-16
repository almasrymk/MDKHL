using FluentValidation;
using SmartParking.Localization;
using SmartParking.UnitPackages;
using SmartParking.Units;

namespace SmartParking.Validations.Units;

public class UnitValidationDto : AbstractValidator<CreateUpdateUnitDto>
{
    public UnitValidationDto()
    {
        
        RuleFor(x => x.ArabicName)
        .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
        .Matches(ConstantValidations.ArabicName).WithMessage(LocalizationHelper.GetLocalizedString("ArabicNameValidtion"));

        RuleFor(x => x.EnglishName)
            .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
            .Matches(ConstantValidations.EnglishName).WithMessage(LocalizationHelper.GetLocalizedString("EnglishNameValidtion"));

    
    }
}

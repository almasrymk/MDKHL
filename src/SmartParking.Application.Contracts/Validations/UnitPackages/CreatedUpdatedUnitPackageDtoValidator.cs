using FluentValidation;
using SmartParking.Localization;
using SmartParking.UnitPackages;

namespace SmartParking.Validations.UnitPackages;

public class CreatedUpdatedUnitPackageDtoValidator : AbstractValidator<CreateUpdateUnitPackageDto>
{
    public CreatedUpdatedUnitPackageDtoValidator()
    {
        RuleFor(x => x.Package.ArabicName)
        .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
        .Matches(ConstantValidations.ArabicName).WithMessage(LocalizationHelper.GetLocalizedString("ArabicNameValidtion"));

        RuleFor(x => x.Package.EnglishName)
            .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
            .Matches(ConstantValidations.EnglishName).WithMessage(LocalizationHelper.GetLocalizedString("EnglishNameValidtion"));

    }
}

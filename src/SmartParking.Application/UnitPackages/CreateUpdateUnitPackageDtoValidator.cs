using FluentValidation;
using SmartParking.Localization;

namespace SmartParking.UnitPackages;

public class CreateUpdateUnitPackageDtoValidator : AbstractValidator<CreateUpdateUnitPackageDto>
{

    public CreateUpdateUnitPackageDtoValidator()
    {
        RuleFor(x => x.NumberOfSlots)
            .GreaterThan(0)
            .WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));

        RuleFor(x => x.NumberOfVehicles)
            .GreaterThan(0)
            .WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
            .GreaterThanOrEqualTo(x => x.NumberOfSlots)
            .WithMessage("Number of vehicles must be greater than or equal to number of slots.");
    }
}
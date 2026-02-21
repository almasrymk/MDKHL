using FluentValidation;

namespace SmartParking.UnitPackages;

public class CreateUpdateUnitPackageDtoValidator : AbstractValidator<CreateUpdateUnitPackageDto>
{
    public CreateUpdateUnitPackageDtoValidator()
    {
        RuleFor(x => x.NumberOfSlots)
            .GreaterThan(-1);
        RuleFor(x => x.NumberOfSlots)
            .GreaterThan(-1);
    }
}
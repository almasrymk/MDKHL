using FluentValidation;
using SmartParking.Cameras.Watchers.Dispensers;
using SmartParking.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Validations.Dispenser;

public class CreateUpdateDispenserDtoValidator : AbstractValidator<CreateUpdateDispenserDto>
{
    public CreateUpdateDispenserDtoValidator()
    {
        RuleFor(dis=>dis.Name)
            .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));
    }
}

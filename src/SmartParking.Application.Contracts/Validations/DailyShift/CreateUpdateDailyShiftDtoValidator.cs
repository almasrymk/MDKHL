using FluentValidation;
using Microsoft.Extensions.Localization;
using SmartParking.Cutomers;
using SmartParking.DailyShifts;
using SmartParking.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Validations.DailyShift;
public class CreateUpdateDailyShiftDtoValidator : AbstractValidator<CreateUpdateDailyShiftDto>
{
  

    public CreateUpdateDailyShiftDtoValidator( )
    {
      
        RuleFor(x => x.GarageId)
        .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));
        RuleFor(x => x.TenantId)
        .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));
        RuleFor(x => x.OrganizationId)
        .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));
        RuleFor(x => x.KeeperId)
        .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));

    }
}

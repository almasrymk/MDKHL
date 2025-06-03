using FluentValidation;
using SmartParking.Authentication;
using SmartParking.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Validations.User;

public class CreatedUpdatedUserDtoValidator : AbstractValidator<UserCreateDto>
{
    public CreatedUpdatedUserDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
            .Matches(@"^(([^<>()\[\]\\.,;:\s@\""]+(\.[^<>()\[\]\\.,;:\s@\""]+)*)|(\"".+\""))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$")
            .WithMessage(LocalizationHelper.GetLocalizedString("EmailValidation"));

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));

        RuleFor(x => x.IdentificationTypes)
            .IsInEnum()
            .WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));

        RuleFor(x => x.NationalId)
            .NotEmpty()
            .WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));

        RuleFor(x => x.UserType)
            .NotNull()
            .WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));
    }
}


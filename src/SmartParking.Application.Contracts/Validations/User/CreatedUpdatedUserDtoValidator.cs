using FluentValidation;
using SmartParking.Authentication;
using SmartParking.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SmartParking.Validations.User;

public class CreatedUpdatedUserDtoValidator : AbstractValidator<UserCreateDto>
{
    public CreatedUpdatedUserDtoValidator()
    {

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage(LocalizationHelper.GetLocalizedString("WrongEmail")).When(e => !e.Email.IsNullOrWhiteSpace())
            .Must(NotContainArabicLetters).WithMessage(LocalizationHelper.GetLocalizedString("WrongEmail")).When(e => !e.Email.IsNullOrWhiteSpace())
            .Must(HaveContentAfterAt).WithMessage(LocalizationHelper.GetLocalizedString("WrongEmail")).When(e => !e.Email.IsNullOrWhiteSpace())
            .Must(EndWithLetter).WithMessage(LocalizationHelper.GetLocalizedString("WrongEmail")).When(e => !e.Email.IsNullOrWhiteSpace());


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
    private bool NotContainArabicLetters(string email)
    {
        // Arabic letters Unicode range: \u0600-\u06FF
        return !Regex.IsMatch(email, @"[\u0600-\u06FF]");
    }

    private bool HaveContentAfterAt(string email)
    {
        // Check that there's content after '@'
        var atIndex = email.LastIndexOf('@');
        return atIndex >= 0 && atIndex < email.Length - 1;
    }

    private bool EndWithLetter(string email)
    {
        // Check if the last character is a letter
        return char.IsLetter(email[email.Length - 1]);
    }
}


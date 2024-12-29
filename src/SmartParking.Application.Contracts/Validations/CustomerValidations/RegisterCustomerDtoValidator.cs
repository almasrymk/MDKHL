using FluentValidation;
using Microsoft.Extensions.Localization;
using SmartParking.Cutomers;
using SmartParking.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SmartParking.Validations.CustomerValidations;

public class RegisterCustomerDtoValidator : AbstractValidator<CreateUpdateCustomerDto>
{
    private readonly IStringLocalizer<SmartParkingResource> _stringLocalizer;
    
    public RegisterCustomerDtoValidator(IStringLocalizer<SmartParkingResource> stringLocalizer)
    {
        _stringLocalizer = stringLocalizer;

        RuleFor(x => x.ArabicName)
        .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage(LocalizationHelper.GetLocalizedString("WrongEmail")).When(e => !e.Email.IsNullOrWhiteSpace())
            .Must(NotContainArabicLetters).WithMessage(LocalizationHelper.GetLocalizedString("WrongEmail")).When(e => !e.Email.IsNullOrWhiteSpace())
            .Must(HaveContentAfterAt).WithMessage(LocalizationHelper.GetLocalizedString("WrongEmail")).When(e => !e.Email.IsNullOrWhiteSpace())
            .Must(EndWithLetter).WithMessage(LocalizationHelper.GetLocalizedString("WrongEmail")).When(e => !e.Email.IsNullOrWhiteSpace());

        RuleFor(x => x.Password)
            .Matches(x=>x.RepeatPassword)
            .WithMessage(LocalizationHelper.GetLocalizedString("WrongPassword"));
        RuleFor(x => x.RepeatPassword).Matches(x=>x.Password).WithMessage("Repeat Password" + LocalizationHelper.GetLocalizedString("WrongPassword"));
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .Matches(@"^(05\d{8}|\+966\d{8})$")  
            .WithMessage(LocalizationHelper.GetLocalizedString("PhoneValidation"));
        RuleFor(x => x.isActive)
            .Must(value => value == true || value == false)
            .WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));
        RuleFor(x=>x.IdentificationType).Must(value => Enum.IsDefined(typeof(Enums.IdentificationTypes), value))
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

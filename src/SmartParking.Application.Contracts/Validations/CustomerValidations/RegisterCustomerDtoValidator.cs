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
    private readonly IStringLocalizer<SmartParkingResource> _localizer;
    
    public RegisterCustomerDtoValidator(IStringLocalizer<SmartParkingResource> localizer)
    {
        _localizer = localizer;

        RuleFor(x => x.ArabicName)
    .NotEmpty()
    .Matches(@"^([\u0600-\u06FF]+\s+){2}[\u0600-\u06FF]+$")
    .WithMessage(_localizer["ArabicName must contain at least three Arabic words."])
    .MinimumLength(10)
    .WithMessage("ArabicName must be at least 10 characters long.");

        RuleFor(x => x.FullName)
            .NotEmpty()
            .Matches(@"^([a-zA-Z]+\s+){2}[a-zA-Z]+$")
            .WithMessage("FullName must contain at least three English words.")
            .MinimumLength(10)
            .WithMessage("FullName must be at least 10 characters long.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not valid.")
            .Must(NotContainArabicLetters).WithMessage("Email should not contain Arabic letters.")
            .Must(HaveContentAfterAt).WithMessage("Email should contain content after '@'.")
            .Must(EndWithLetter).WithMessage("Email should end with a letter.");

        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required").MinimumLength(8).WithMessage("Password is minimum length is 8").Matches(x=>x.RepeatPassword).WithMessage("Password must match repeat password");
        RuleFor(x => x.RepeatPassword).NotEmpty().WithMessage("Password is required").MinimumLength(8).WithMessage("Password is minimum length is 8").Matches(x=>x.Password).WithMessage("RepeatPassword must match Password");
        RuleFor(x => x.MobileNumber).NotEmpty().Matches(@"^\d{10,15}$");
        RuleFor(x => x.isActive)
            .Must(value => value == true || value == false)
            .WithMessage("The IsActive field must be either true (1) or false (0).");
        RuleFor(x=>x.IdentificationType).Must(value => Enum.IsDefined(typeof(Enums.IdentificationTypes), value))
             .WithMessage("The value must be NationalID(1) or Passport(0).");

        
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

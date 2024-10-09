using FluentValidation;
using SmartParking.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Validations.Organizations;

public class CreatedUpdatedOrganizationDtoValidator : AbstractValidator<CreateUpdateOrganiztionDto>
{
    public CreatedUpdatedOrganizationDtoValidator()
    {
        RuleFor(x=>x.UnifiedLicenseNum).Length(9).NotEmpty().Matches(@"^920[^\s,]*$");
        RuleFor(x => x.ArabicName)
        .Matches(@"^[\p{IsArabic}][\p{IsArabic}\d،\.\,\/\\\s]*$")
        .WithMessage("The Arabic name should only contain Arabic letters, digits, punctuation marks, and spaces.");

        RuleFor(x => x.CRExpiryDate)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("The expiry date cannot be earlier than today.");

        RuleFor(x => x.LicenseExpiryDate)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("The expiry date cannot be earlier than today.");

        RuleFor(x => x.EnglishName)
            .Matches(@"^[a-zA-Z][a-zA-Z0-9/.,\\]*(\s[a-zA-Z0-9/.,\\]+)*$");

        RuleFor(x => x.WhatsAppNum).Matches(@"^(?:05[3-8]\d{7}|\+966[3-8]\d{8})$");

        RuleFor(x => x.ContactMail).Matches(@"^(?!.*\.\.)(?!.*\.$)(?=.{1,254}$)(?=.{1,64}@.{1,255}$)[a-zA-Z0-9._-]+@[a-zA-Z0-9-]+\.(com|net|org|edu|gov|mil|int|co|info|biz|us|eu|uk|ca|jp|cn|in|au|za|tv|me|mobi|name)$");
        RuleFor(x => x.PhoneNum).Matches(@"^\d+$").WithMessage("The field must contain only numbers.").MinimumLength(9).WithMessage("Phone Number Can't be less than 9 digits").MaximumLength(10).WithMessage("Phone Number Can't be less than 9 digits");

        RuleFor(x => x.SubscriptionId)
            .Must(id => id == null || id > 0)
            .WithMessage("SubscriptionId must be a positive number if provided.");
    }
}

using Castle.Core.Internal;
using FluentValidation;
using Newtonsoft.Json.Linq;
using SmartParking.Localization;
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
        RuleFor(x => x.CommercialRegistrationNumber)
            .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
            .Length(10);
        
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
            .Matches(@"^[a-zA-Z][a-zA-Z0-9/.,\\@#*]*(\s[a-zA-Z0-9/.,\\@#*]+)*$").WithMessage(LocalizationHelper.GetLocalizedString("EnglishNameValidtion"));

        RuleFor(x => x.NationalId)
     .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));


        RuleFor(x => x.ArabicName)
        .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
        .Matches(@"^[\p{IsArabic}][\p{IsArabic}\d،\.\,\/\\\s@#\*\-]*$").WithMessage(LocalizationHelper.GetLocalizedString("ArabicNameValidtion"));


        RuleFor(x => x.CRExpiryDate)
        .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
        .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today)).WithMessage(LocalizationHelper.GetLocalizedString("ExpiryDateValidation"));

        RuleFor(x => x.EnglishName)
            .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
            .Matches(@"^[a-zA-Z][a-zA-Z0-9/.,\\@#*]*(\s[a-zA-Z0-9/.,\\@#*]+)*$").WithMessage(LocalizationHelper.GetLocalizedString("EnglishNameValidtion"));


        RuleFor(x => x.WhatsAppNum)
            .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
            .Matches(@"^(05\d{8}|\+9665\d{8})$").WithMessage(LocalizationHelper.GetLocalizedString("WhatsAppValidation"));

		RuleFor(x => x.Email)
            .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
            .Matches(@"^(?!.*\.\.)(?!.*\.$)(?=.{1,254}$)(?=.{1,64}@.{1,255}$)[a-zA-Z0-9._-]+@[a-zA-Z0-9-]+\.(com|net|org|edu|gov|mil|int|co|info|biz|us|eu|uk|ca|jp|cn|in|au|za|tv|me|mobi|name)$")
            .WithMessage(LocalizationHelper.GetLocalizedString("EmailValidation"));


        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
            .Matches(@"^01\d{8}$").WithMessage(LocalizationHelper.GetLocalizedString("OrganizationPhoneValidation"));

        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"));


        RuleFor(x=>x.FirstHourPrice)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.SecondHourPrice)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.ThirdHourPrice)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.AdditionalHourPrice)
           .GreaterThanOrEqualTo(0);

        RuleFor(x => x.FullDayPrice)
          .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
          .GreaterThanOrEqualTo(0);

        RuleFor(x => x.NumberOfHours)
         .GreaterThanOrEqualTo(0);

        RuleFor(x => x.BuildingNumber)
            .NotEmpty()
            .InclusiveBetween(1, 9999);

        RuleFor(x => x.PostalCode)
            .NotEmpty()
            .InclusiveBetween(11111, 99999);
        
        RuleFor(x => x.SecondaryNumber)
            .NotEmpty()
            .InclusiveBetween(1111, 9999);

        RuleFor(x => x.FreeMinutes)
           .GreaterThanOrEqualTo(0);


        RuleFor(x=>x.District)
            .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
            .Matches(@"^[A-Za-z\u0600-\u06FF\s]{3,20}$")
           .MaximumLength(20).WithMessage(LocalizationHelper.GetLocalizedString("StreetValidation"));

        RuleFor(x => x.StreetName)
            .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
           .Matches(@"^[A-Za-z\u0600-\u06FF\s]{3,20}$")
           .MaximumLength(20).WithMessage(LocalizationHelper.GetLocalizedString("StreetValidation"));

        RuleFor(x => x.TaxNumber)
           .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
           .Length(15).WithMessage(LocalizationHelper.GetLocalizedString("RangeValidation") + " " + 15)
           .Matches(@"^\d{15}$").WithMessage(LocalizationHelper.GetLocalizedString("OnlyEnglishNumberAllowed"));
    }
}

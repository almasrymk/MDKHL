using Castle.Core.Internal;
using FluentValidation;
using Newtonsoft.Json.Linq;
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
        RuleFor(x=>x.CommercialRegistrationNumber).NotEmpty().Length(10);
        RuleFor(x => x.ArabicName)
        .NotEmpty()
        .Matches(@"^[\p{IsArabic}][\p{IsArabic}\d،\.\,\/\\\s@#\*\-]*$")
        .WithMessage("The Arabic name should only contain Arabic letters, digits, punctuation marks, and spaces.");

        RuleFor(x => x.CRExpiryDate)
            .NotEmpty()
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("The expiry date cannot be earlier than today.");

        RuleFor(x => x.LicenseExpiryDate)
            .NotEmpty()
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("The expiry date cannot be earlier than today.");

        RuleFor(x => x.EnglishName)
            .NotEmpty()
            .Matches(@"^[a-zA-Z][a-zA-Z0-9/.,\\@#*]*(\s[a-zA-Z0-9/.,\\@#*]+)*$");

		RuleFor(x => x.WhatsAppNum)
			.NotEmpty()
			.Matches(@"^(05[3-8]\d{7}|\+966[3-8]\d{8})$")
			.WithMessage("please add valid whats app number");
		RuleFor(x => x.ContactMail).NotEmpty().Matches(@"^(?!.*\.\.)(?!.*\.$)(?=.{1,254}$)(?=.{1,64}@.{1,255}$)[a-zA-Z0-9._-]+@[a-zA-Z0-9-]+\.(com|net|org|edu|gov|mil|int|co|info|biz|us|eu|uk|ca|jp|cn|in|au|za|tv|me|mobi|name)$");
        RuleFor(x => x.PhoneNum).NotEmpty().Matches(@"^\d+$").WithMessage("The field must contain only numbers.").MinimumLength(9).WithMessage("Phone Number Can't be less than 9 digits").MaximumLength(10).WithMessage("Phone Number Can't be less than 9 digits");

        RuleFor(x => x.SubscriptionId)
            .Must(id => id == null || id > 0)
            .WithMessage("SubscriptionId must be a positive number if provided.");

        RuleFor(x=>x.FirstHourPrice)
            .NotEmpty()
            .GreaterThan(0);

        RuleFor(x => x.SecondHourPrice)
            .NotEmpty()
            .GreaterThan(0);

        RuleFor(x => x.ThirdHourPrice)
            .NotEmpty()
            .GreaterThan(0);

        RuleFor(x => x.AdditionalHourPrice)
           .NotEmpty()
           .GreaterThan(0);

        RuleFor(x => x.FullDayPrice)
          .NotEmpty()
          .GreaterThan(0);

        RuleFor(x => x.NumberOfHours)
            .NotEmpty()
          .GreaterThan(0);

        RuleFor(x => x.BuildingNumber)
            .NotEmpty()
            .InclusiveBetween(1, 9999);

        RuleFor(x => x.PostalCode)
            .NotEmpty()
            .InclusiveBetween(11111, 99999);
        
        RuleFor(x => x.SecondaryNumber)
            .NotEmpty()
            .InclusiveBetween(1111, 9999);

        RuleFor(x=>x.District)
            .NotEmpty()
            .Matches(@"^[A-Za-z\u0600-\u06FF\s]{3,20}$")
            .MaximumLength(20)
            .WithMessage("District must start with 3 to 20 English or Arabic letters, and the total length must not exceed 25 characters.");

        RuleFor(x => x.StreetName)
           .NotEmpty()
           .Matches(@"^[A-Za-z\u0600-\u06FF\s]{3,20}$") 
           .MaximumLength(20)
           .WithMessage("Street Name must start with 3 to 20 English or Arabic letters, spaces are allowed, and the total length must not exceed 20 characters.");



        //RuleFor(x => x.IsNumberOfHoursExceeded)
        //.Must(v=>v == true || v == false)
        //.WithMessage("The value must be either true or false.");

        //RuleFor(x => x.IsFullDayExceeded)
        // .Must(v => v == true || v == false)
        // .WithMessage("The value must be either true or false."); ;


    }
}

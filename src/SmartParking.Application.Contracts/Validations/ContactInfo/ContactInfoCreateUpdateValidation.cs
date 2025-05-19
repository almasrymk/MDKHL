using FluentValidation;
using SmartParking.Localization;
using SmartParking.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Validations.ContactInfo;

public class ContactInfoCreateUpdateValidation : AbstractValidator<ContactInfoCreateUpdateDto>
{

    public ContactInfoCreateUpdateValidation()
    {
        RuleFor(x => x.WhatsAppNum)
         .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
         .Matches(ConstantValidations.WhatsAppNumber).WithMessage(LocalizationHelper.GetLocalizedString("WhatsAppValidation"));


        RuleFor(x => x.ContactMail)
             .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
            .Matches(ConstantValidations.Email)
            .WithMessage(LocalizationHelper.GetLocalizedString("EmailValidation"));

        RuleFor(x => x.PhoneNum)
           .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
           .Matches(ConstantValidations.PhoneNumber).WithMessage(LocalizationHelper.GetLocalizedString("OrganizationPhoneValidation"));

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
            .Matches(ConstantValidations.EnglishName).WithMessage(LocalizationHelper.GetLocalizedString("EnglishNameValidtion"));


    }
}

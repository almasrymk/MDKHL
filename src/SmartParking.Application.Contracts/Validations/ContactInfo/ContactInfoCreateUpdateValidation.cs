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
         .Matches(@"^(05\d{8}|\+9665\d{8})$").WithMessage(LocalizationHelper.GetLocalizedString("WhatsAppValidation"));


        RuleFor(x => x.ContactMail)
             .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
            .Matches(@"^(?!.*\.\.)(?!.*\.$)(?=.{1,254}$)(?=.{1,64}@.{1,255}$)[a-zA-Z0-9._-]+@[a-zA-Z0-9-]+\.(com|net|org|edu|gov|mil|int|co|info|biz|us|eu|uk|ca|jp|cn|in|au|za|tv|me|mobi|name)$")
            .WithMessage(LocalizationHelper.GetLocalizedString("EmailValidation"));

        RuleFor(x => x.PhoneNum)
           .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
           .Matches(@"^01\d{8}$").WithMessage(LocalizationHelper.GetLocalizedString("PhoneValidation"));

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(LocalizationHelper.GetLocalizedString("EmptyValidation"))
            .Matches(@"^[a-zA-Z][a-zA-Z0-9/.,\\@#*]*(\s[a-zA-Z0-9/.,\\@#*]+)*$").WithMessage(LocalizationHelper.GetLocalizedString("EnglishNameValidtion"));


    }
}

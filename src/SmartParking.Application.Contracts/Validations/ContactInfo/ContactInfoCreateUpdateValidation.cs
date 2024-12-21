using FluentValidation;
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
        .NotEmpty()
        .Matches(@"^(05[3-8]\d{7}|\+966[3-8]\d{8})$")
        .WithMessage("please add valid whats app number");
        RuleFor(x => x.ContactMail).NotEmpty().Matches(@"^(?!.*\.\.)(?!.*\.$)(?=.{1,254}$)(?=.{1,64}@.{1,255}$)[a-zA-Z0-9._-]+@[a-zA-Z0-9-]+\.(com|net|org|edu|gov|mil|int|co|info|biz|us|eu|uk|ca|jp|cn|in|au|za|tv|me|mobi|name)$");
        RuleFor(x => x.PhoneNum)
            .NotEmpty()
            .Matches(@"^01\d{8}$")
            .WithMessage("Phone Number must start with '01', contain only numbers, and be exactly 10 digits long.");
        RuleFor(x => x.Name)
            .NotEmpty()
            .Matches(@"^[a-zA-Z][a-zA-Z0-9/.,\\@#*]*(\s[a-zA-Z0-9/.,\\@#*]+)*$");


    }
}

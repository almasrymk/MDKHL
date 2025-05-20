using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Validations;

public static class ConstantValidations
{
    public const string ArabicName = @"^[\p{IsArabic}][\p{IsArabic}\d،.,/\\\s@#*\-]*$";
    public const string EnglishName = @"^[a-zA-Z][a-zA-Z0-9/.,\\@#*]*(\s[a-zA-Z0-9/.,\\@#*]+)*$";
    public const string WhatsAppNumber = @"^(05\d{8}|\+9665\d{8})$";
    public const string Email = @"^(?!.*\.\.)(?!.*\.$)(?=.{1,254}$)(?=.{1,64}@.{1,255}$)[a-zA-Z0-9._-]+@[a-zA-Z0-9-]+\.(com|net|org|edu|gov|mil|int|co|info|biz|us|eu|uk|ca|jp|cn|in|au|za|tv|me|mobi|name)$";
    public const string PhoneNumber = @"^(01|05)\d{8}$";



}

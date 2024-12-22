using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using SmartParking.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Validations;

public static class VehicleValidation 
{
    

    public static string ValidateArabicLetters(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null; 
        }

       
        var arabicLetterPattern = @"^[\u0600-\u06FF]+$";
        if (!System.Text.RegularExpressions.Regex.IsMatch(value, arabicLetterPattern))
        {
            return LocalizationHelper.GetLocalizedString("OnlyArabicLettersAllowed");
        }

        return null; 
    }
    
    public static string ValidateArabicNumber(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null; 
        }

       
        var arabicLetterPattern = @"^[\u0660-\u0669]+$";
        if (!System.Text.RegularExpressions.Regex.IsMatch(value, arabicLetterPattern))
        {
            return LocalizationHelper.GetLocalizedString("OnlyArabicNumberAllowed");
        }

        return null; 
    }
    public static string ValidateLetters(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        // Pattern to validate only letters (Arabic or English)
        var letterPattern = @"^[a-zA-Z\u0600-\u06FF]+$"; // a-zA-Z for English letters, \u0600-\u06FF for Arabic letters
        if (!System.Text.RegularExpressions.Regex.IsMatch(value, letterPattern))
        {
            return LocalizationHelper.GetLocalizedString("OnlyLettersAllowed");
        }

        return null;
    }
    public static string ValidateEnglishLetter(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null; 
        }

       
        var arabicLetterPattern = @"^[a-zA-Z]+$";
        if (!System.Text.RegularExpressions.Regex.IsMatch(value, arabicLetterPattern))
        {
            return LocalizationHelper.GetLocalizedString("OnlyEnglishLettersAllowed");
        }

        return null; 
    }
    
    public static string ValidateEnglishNumber(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null; 
        }

       
        var arabicLetterPattern = @"^[0-9]+$";
        if (!System.Text.RegularExpressions.Regex.IsMatch(value, arabicLetterPattern))
        {
            return LocalizationHelper.GetLocalizedString("OnlyEnglishNumberAllowed");
        }

        return null; 
    }
}

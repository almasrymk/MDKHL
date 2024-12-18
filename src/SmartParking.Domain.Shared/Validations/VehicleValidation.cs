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
    private static IServiceProvider _serviceProvider;

    public static void Configure(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public static string GetLocalizedString(string key)
    {
        var localizer = _serviceProvider.GetRequiredService<IStringLocalizer<SmartParkingResource>>();
        return localizer[key];
    }

    public static string ValidateArabicLetters(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null; 
        }

       
        var arabicLetterPattern = @"^[\u0600-\u06FF]+$";
        if (!System.Text.RegularExpressions.Regex.IsMatch(value, arabicLetterPattern))
        {
            return GetLocalizedString("OnlyArabicLettersAllowed");
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
            return GetLocalizedString("OnlyArabicNumberAllowed");
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
            return GetLocalizedString("OnlyEnglishLettersAllowed");
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
            return GetLocalizedString("OnlyEnglishNumberAllowed");
        }

        return null; 
    }
}

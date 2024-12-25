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

    public static string ValidateYear(string year)
    {
        if (int.Parse(year) > DateTime.Now.Year)
        {
            return LocalizationHelper.GetLocalizedString("NotValidYear");
        }
        return null;
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
    
    public static string ValidateNumber(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null; 
        }

       
        var arabicLetterPattern = @"^[0-9\u0660-\u0669]+$";
        if (!System.Text.RegularExpressions.Regex.IsMatch(value, arabicLetterPattern))
        {
            return LocalizationHelper.GetLocalizedString("OnlyNumberAllowed");
        }

        return null; 
    }
    public static (string,string) HandleKeyUpLetters(string plateLetters)
    {
        if (plateLetters == "")
            return ("","");
        string ArPlateLetters = "";
        string EnPlateLetters = "";
        if (plateLetters.All(c => c >= '\u0600' && c <= '\u06FF'))
        {
            ArPlateLetters = plateLetters;
            EnPlateLetters = MapArabicToEnglish(plateLetters);
            if (EnPlateLetters == "")
            {
                plateLetters = "";
                ArPlateLetters = "";
            }
        }
        else if (plateLetters.All(c => (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')))
        {
            EnPlateLetters = plateLetters.ToUpper();
            ArPlateLetters = MapEnglishToArabic(plateLetters);
            if (ArPlateLetters == "")
            {
                plateLetters = "";
                EnPlateLetters = "";
            }
        }
        else
        {
           throw new Exception ("Letters can't be mixed");
        }
        return (EnPlateLetters, ArPlateLetters);


    }

    private static string MapArabicToEnglish(string arabicText)
    {
        var ArabicToEnglishMap = new Dictionary<char, char>
{
    {'ا', 'A'}, {'ب', 'B'}, {'د', 'D'}, {'ح', 'J'}, {'ر', 'R'}, {'س', 'S'}, {'ص', 'X'}, {'ط', 'T'},
    {'ع', 'E'}, {'ق', 'G'}, {'ك', 'K'}, {'ل', 'L'}, {'ن', 'N'}, {'م', 'Z'}, {'ه', 'H'}, {'و', 'U'}, {'ى', 'V'}
};


        StringBuilder result = new StringBuilder();
        foreach (var charInArabic in arabicText)
        {
            if (ArabicToEnglishMap.ContainsKey(charInArabic))
            {
                result.Append(ArabicToEnglishMap[charInArabic]);
            }
            else
            {
                throw new Exception("Invalid PlateLetter");
            }
        }
        return result.ToString();
    }

    private static string MapEnglishToArabic(string englishText)
    {
        var EnglishToArabicMap = new Dictionary<char, char>
      {
          {'A', 'ا'}, {'B', 'ب'}, {'D', 'د'},{'J', 'ح'},{'R', 'ر'}, {'S', 'س'},{'X', 'ص'},{'T', 'ط'},
          {'E', 'ع'}, {'G', 'ق'},{'K', 'ك'}, {'L', 'ل'},{'N', 'ن'},{'Z', 'م'},{'H', 'ه'},{'U', 'و'},{'V', 'ى'}
      };
        englishText = englishText.ToUpper();
        StringBuilder result = new StringBuilder();
        foreach (var charInEnglish in englishText)
        {
            if (EnglishToArabicMap.ContainsKey(charInEnglish))
            {
                result.Append(EnglishToArabicMap[charInEnglish]);
            }
            else
            {
                throw new Exception("Invalid PlateLetter");

            }
        }
        return result.ToString();
    }

    public static (string,string) HandleKeyUp(string e)
    {
        string EnglishNumbers = "";
        string ArabicNumbers = "";
        if (e.All(c => c >= '٠' && c <= '٩'))
        {
            ArabicNumbers = e;
            EnglishNumbers=MapToEnglishNumerals(e);
      
        }
        else if (e.All(c => (c >= '0' && c <= '9')))
        {
            EnglishNumbers = e;
            ArabicNumbers=MapToArabicNumerals(e);
        }
        else
        {
            throw new Exception("Letters can't be mixed");
        }
        return(EnglishNumbers, ArabicNumbers);

    }


    private static string MapToArabicNumerals(string englishNumber)
    {
        if (englishNumber == "")
        {
            return "";
        }
        var englishToArabicMap = new Dictionary<char, string>
         {
            { '0', "٠" },
            { '1', "١" },
            { '2', "٢" },
            { '3', "٣" },
            { '4', "٤" },
            { '5', "٥" },
            { '6', "٦" },
            { '7', "٧" },
            { '8', "٨" },
            { '9', "٩" }
        };
        string arabicNUmbers = "";
        foreach (var character in englishNumber)
        {

            if (character < '0' || character > '9')
            {
                throw new Exception("Only Numbers are accepted");

            }
            arabicNUmbers += englishToArabicMap[character];
        }
        return arabicNUmbers;
    }

        private static string MapToEnglishNumerals(string arabicNumber)
        {
           if (arabicNumber == "")
           {
               return "";
           }
           var arabicToEnglishMap = new Dictionary<char, string>
            {
               { '٠',"0" },
               {'١',"1"},
               { '٢',"2" },
               { '٣',"3" },
               { '٤' ,"4"},
               { '٥',"5" },
               { '٦',"6" },
               { '٧',"7" },
               { '٨' ,"8"},
               { '٩',"9" }
           };
           string englishNUmbers = "";
           foreach (var character in arabicNumber)
           {

               if (character < '٠' || character > '٩')
               {
                   throw new Exception("Only Numbers are accepted");

               }
               englishNUmbers += arabicToEnglishMap[character];
           }
           return englishNUmbers;
    }



}

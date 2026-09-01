using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using SmartParking.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartParking.IntegrationUnit.Dtos;

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
            return LocalizationHelper.GetLocalizedString("OnlyEnglishNumberAllowed");
        }

        return null;
    }
    public static (string, string) HandleKeyUpLetters(string plateLetters)
    {
        if (plateLetters == "")
            return ("", "");
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
            throw new Exception(LocalizationHelper.GetLocalizedString("MixedInvalid"));
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
                throw new Exception(LocalizationHelper.GetLocalizedString("NotFoundValidation") + ' ' + LocalizationHelper.GetLocalizedString("PlateLetter") + ' ' + charInArabic);
            }
        }
        return ReversStringBuilder(result);
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
                throw new Exception(LocalizationHelper.GetLocalizedString("NotFoundValidation") + ' ' + LocalizationHelper.GetLocalizedString("PlateLetter") + ' ' + charInEnglish);

            }
        }
        return ReversStringBuilder(result);
    }

    private static string ReversStringBuilder(StringBuilder result)
    {
        var resultText = result.ToString();
        return new string(resultText.Reverse().ToArray());
    }

    public static (string, string) HandleKeyUp(string e)
    {
        string EnglishNumbers = "";
        string ArabicNumbers = "";
        if (e.All(c => c >= '٠' && c <= '٩'))
        {
            ArabicNumbers = e;
            EnglishNumbers = MapToEnglishNumerals(e);

        }
        else if (e.All(c => (c >= '0' && c <= '9')))
        {
            EnglishNumbers = e;
            ArabicNumbers = MapToArabicNumerals(e);
        }
        else
        {
            throw new Exception(LocalizationHelper.GetLocalizedString("MixedInvalid"));
        }
        return (EnglishNumbers, ArabicNumbers);

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
                throw new Exception(LocalizationHelper.GetLocalizedString("OnlyEnglishNumberAllowed"));

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
                throw new Exception(LocalizationHelper.GetLocalizedString("OnlyArabicNumberAllowed"));

            }
            englishNUmbers += arabicToEnglishMap[character];
        }
        return englishNUmbers;
    }
    public static bool CheckPlateNumberCriteria(string ArabicPlateNumber, string EnglishPlateNumber)
    {
        string arabicLetterPattern = @"[\u0600-\u06FF]";
        string arabicDigitPattern = @"[\u0660-\u0669]";

        string englishLetterPattern = @"[A-Za-z]";
        string englishDigitPattern = @"[0-9]";

        bool isArabicPlateValid = !string.IsNullOrWhiteSpace(ArabicPlateNumber) &&
                                  System.Text.RegularExpressions.Regex.IsMatch(ArabicPlateNumber, arabicLetterPattern) &&
                                  System.Text.RegularExpressions.Regex.IsMatch(ArabicPlateNumber, arabicDigitPattern);

        bool isEnglishPlateValid = !string.IsNullOrWhiteSpace(EnglishPlateNumber) &&
                                   System.Text.RegularExpressions.Regex.IsMatch(EnglishPlateNumber, englishLetterPattern) &&
                                   System.Text.RegularExpressions.Regex.IsMatch(EnglishPlateNumber, englishDigitPattern);

        return isArabicPlateValid && isEnglishPlateValid;

    }

    public static VehiclePlateDto Parse(string plate)
    {
        if (string.IsNullOrWhiteSpace(plate))
        {
            return new VehiclePlateDto
            {
                EnPlateNumber = string.Empty,
                ArPlateNumber = string.Empty,
                EnPlateLetters = string.Empty,
                ArPlateLetters = string.Empty
            };
        }

        StringBuilder numBuilder = new StringBuilder();
        StringBuilder letBuilder = new StringBuilder();

        bool hasEnDigit = false;
        bool hasArDigit = false;
        bool hasEnLetter = false;
        bool hasArLetter = false;

        foreach (char c in plate)
        {
            if (char.IsWhiteSpace(c) || c == '-' || c == '_')
                continue;

            if (c >= '0' && c <= '9')
            {
                numBuilder.Append(c);
                hasEnDigit = true;
            }
            else if (c >= '٠' && c <= '٩')
            {
                numBuilder.Append(c);
                hasArDigit = true;
            }
            else if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
            {
                letBuilder.Append(c);
                hasEnLetter = true;
            }
            else if (c >= '\u0600' && c <= '\u06FF')
            {
                letBuilder.Append(c);
                hasArLetter = true;
            }
            else
            {
                throw new Exception(LocalizationHelper.GetLocalizedString("MixedInvalid"));
            }
        }

        if ((hasEnDigit && hasArLetter) || (hasArDigit && hasEnLetter))
        {
            throw new Exception(LocalizationHelper.GetLocalizedString("MixedInvalid"));
        }

        string enNum = string.Empty;
        string arNum = string.Empty;
        string enLet = string.Empty;
        string arLet = string.Empty;

        string numStr = numBuilder.ToString();
        if (!string.IsNullOrEmpty(numStr))
        {
            (enNum, arNum) = HandleKeyUp(numStr);
        }

        string letStr = letBuilder.ToString();
        if (!string.IsNullOrEmpty(letStr))
        {
            (enLet, arLet) = HandleKeyUpLetters(letStr);
            if (!string.IsNullOrEmpty(arLet))
            {
                arLet = string.Join(" ", arLet.Where(c => !char.IsWhiteSpace(c)));
            }
        }

        return new VehiclePlateDto
        {
            EnPlateNumber = enNum,
            ArPlateNumber = arNum,
            EnPlateLetters = enLet,
            ArPlateLetters = arLet
        };
    }

    public static bool IsValidSaudiPlate(string plate)
    {
        if (string.IsNullOrWhiteSpace(plate))
        {
            return false;
        }

        try
        {
            var parsed = Parse(plate);
            if (string.IsNullOrEmpty(parsed.EnPlateLetters) || string.IsNullOrEmpty(parsed.EnPlateNumber))
            {
                return false;
            }

            if (parsed.EnPlateLetters.Length < 1 || parsed.EnPlateLetters.Length > 3 ||
                parsed.EnPlateNumber.Length < 1 || parsed.EnPlateNumber.Length > 4)
            {
                return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsValidTicketNumber(string ticketNumber)
    {
        if (string.IsNullOrWhiteSpace(ticketNumber))
        {
            return false;
        }

        var trimmed = ticketNumber.Trim();
        return System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^(?:[A-Za-z]{3}[0-9]{8}|[0-9]{10,11})$");
    }
}

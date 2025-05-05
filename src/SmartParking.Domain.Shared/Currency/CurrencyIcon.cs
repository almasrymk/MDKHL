using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Currency;

public static class CurrencyIcon
{

    public static string ToHtml(string Class, string FontSize)
    {
        return $"<i class='{Class}' style='font-size: {FontSize};'></i>";
    }
}

using SmartParking.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Localization;

namespace SmartParking.StringLocalizer;

public static class CustomStringLocalizer
{

    public const string Add = "Permission:Create";
    public const string Edit = "Permission:Edit";
    public const string Delete = "Permission:Delete";
    public static LocalizableString L(string name)
    {
        return LocalizableString.Create<SmartParkingResource>(name);
    }
}

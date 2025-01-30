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
    public const string ChanageStatus = "Permission:ChanageStatus";
    public const string AddRoleToCustomer = "Permission:AddRoleToCustomer";
    public const string IT = "Permission:IT";
    public const string SecurityManager = "Permission:SecurityManager";
    public const string FinancialManagement = "Permission:FinancialManagement";
    public static LocalizableString L(string name)
    {
        return LocalizableString.Create<SmartParkingResource>(name);
    }
}

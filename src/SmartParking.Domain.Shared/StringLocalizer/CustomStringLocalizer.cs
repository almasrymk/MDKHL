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
    public const string View = "Permission:View";
    public const string ChanageStatus = "Permission:ChanageStatus";
    public const string AddRoleToCustomer = "Permission:AddRoleToCustomer";
    public const string ViewCustomerCard = "Permission:ViewCustomerCard";
    public const string ViewVisitorCard = "Permission:ViewVisitorCard";
    public const string ViewEmployeeCard = "Permission:ViewEmployeeCard";
    public const string ViewOrganizationCard = "Permission:ViewOrganizationCard";
    public const string ViewGarageCard = "Permission:ViewGarageCard";
    public const string ViewBeneficiaresCard = "Permission:ViewBeneficiaresCard";
    public const string ViewAmountCard = "Permission:ViewAmountCard";
    public const string ViewBarChartCard = "Permission:ViewBarChartCard";
    public const string ViewBieChartCard = "Permission:ViewPieChartCard";
    public const string ViewNoPackageCard = "Permission:ViewNoPackageCard";
    public const string ViewTotalSlotsCard = "Permission:ViewTotalSlotsCard";
    public const string ViewNumberOfEmployeeCard = "Permission:ViewNumberOfEmployeeCard";
    public const string ViewNumberOfCustomerCard = "Permission:ViewNumberOfCustomerCard";
    public const string ViewNumberOfVisitorCard = "Permission:ViewNumberOfVisitorCard";
    public const string IT = "Permission:IT";
    public const string SecurityManager = "Permission:SecurityManager";
    public const string FinancialManagement = "Permission:FinancialManagement";
    public const string TransactionCheckout = "Permission:TransactionCheckout";
    public const char For ='ب';

    public static LocalizableString L(string name)
    {
        return LocalizableString.Create<SmartParkingResource>(name);
    }
}

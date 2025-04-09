using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Permissions.CardsPermissions
{
    public static class CardsPermissions
    {

        public const string PropertyName = ".Dashboard";
        public const string GroupName = SmartParkingPermissions.GroupName;
        public const string Default = GroupName + PropertyName;
        public const string ViewCustomerCard = Default + SmartParkingPermissions.ViewCustomerCard;
        public const string ViewVisitorCard = Default + SmartParkingPermissions.ViewVisitorCard;
        public const string ViewEmployeeCard = Default + SmartParkingPermissions.ViewEmployeeCard;
        public const string ViewOrganizationCard = Default + SmartParkingPermissions.ViewOrganizationCard;
        public const string ViewGarageCard = Default + SmartParkingPermissions.ViewGarageCard;
        public const string ViewBeneficiaresCard = Default + SmartParkingPermissions.ViewBeneficiaresCard;
        public const string ViewBarChartCard = Default + SmartParkingPermissions.ViewBarChartCard;
        public const string ViewBieChartCard = Default + SmartParkingPermissions.ViewBieChartCard;
        public const string ViewAmountCard = Default + SmartParkingPermissions.ViewAmountCard;
        public const string ViewNoPackageCard = Default + SmartParkingPermissions.ViewNoPackageCard;
        public const string ViewTotalSlotsCard = Default + SmartParkingPermissions.ViewTotalSlotsCard;
        public const string ViewNumberOfEmployeeCard = Default + SmartParkingPermissions.ViewNumberOfEmployeeCard;
        public const string ViewNumberOfCustomerCard = Default + SmartParkingPermissions.ViewNumberOfCustomerCard;
        public const string ViewNumberOfVisitorCard = Default + SmartParkingPermissions.ViewNumberOfVisitorCard;
    }
}

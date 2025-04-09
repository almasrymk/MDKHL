using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartParking.StringLocalizer;
using Volo.Abp.Authorization.Permissions;

namespace SmartParking.Permissions.CardsPermissions
{
    public class CardsPermissionDefinitionProvider : PermissionDefinitionProvider
    {
        public override void Define(IPermissionDefinitionContext context)
        {
            var smartParkingGroup = context.AddGroup(CardsPermissions.Default, CustomStringLocalizer.L("Permission:Dashboard"));

            var cardPermission = smartParkingGroup.AddPermission(CardsPermissions.Default, CustomStringLocalizer.L("Permission:Dashboard"));
            cardPermission.AddChild(CardsPermissions.ViewCustomerCard, CustomStringLocalizer.L(CustomStringLocalizer.ViewCustomerCard));
            cardPermission.AddChild(CardsPermissions.ViewEmployeeCard, CustomStringLocalizer.L(CustomStringLocalizer.ViewEmployeeCard));
            cardPermission.AddChild(CardsPermissions.ViewVisitorCard, CustomStringLocalizer.L(CustomStringLocalizer.ViewVisitorCard)); 
            cardPermission.AddChild(CardsPermissions.ViewBeneficiaresCard, CustomStringLocalizer.L(CustomStringLocalizer.ViewBeneficiaresCard));
            cardPermission.AddChild(CardsPermissions.ViewOrganizationCard, CustomStringLocalizer.L(CustomStringLocalizer.ViewOrganizationCard));
            cardPermission.AddChild(CardsPermissions.ViewGarageCard, CustomStringLocalizer.L(CustomStringLocalizer.ViewGarageCard));
            cardPermission.AddChild(CardsPermissions.ViewAmountCard, CustomStringLocalizer.L(CustomStringLocalizer.ViewAmountCard));
            cardPermission.AddChild(CardsPermissions.ViewBarChartCard, CustomStringLocalizer.L(CustomStringLocalizer.ViewBarChartCard));
            cardPermission.AddChild(CardsPermissions.ViewBieChartCard, CustomStringLocalizer.L(CustomStringLocalizer.ViewBieChartCard));
            cardPermission.AddChild(CardsPermissions.ViewNumberOfVisitorCard, CustomStringLocalizer.L(CustomStringLocalizer.ViewNumberOfVisitorCard));
            cardPermission.AddChild(CardsPermissions.ViewNumberOfEmployeeCard, CustomStringLocalizer.L(CustomStringLocalizer.ViewNumberOfEmployeeCard));
            cardPermission.AddChild(CardsPermissions.ViewNumberOfCustomerCard, CustomStringLocalizer.L(CustomStringLocalizer.ViewNumberOfCustomerCard));
            cardPermission.AddChild(CardsPermissions.ViewTotalSlotsCard, CustomStringLocalizer.L(CustomStringLocalizer.ViewTotalSlotsCard));
        }
    }
}

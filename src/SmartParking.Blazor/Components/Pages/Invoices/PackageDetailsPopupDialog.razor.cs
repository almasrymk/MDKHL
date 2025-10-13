using Microsoft.AspNetCore.Components;
using SmartParking.CustomerPackages;
using SmartParking.Packages;

namespace SmartParking.Blazor.Components.Pages.Invoices;

public partial class PackageDetailsPopupDialog
{
    [Parameter] public CustomerPackageDto? Package { get; set; }
    [Parameter] public string OrganizationAddress { get; set; }
    [Parameter] public string OrganizationArabicName { get; set; }
    [Parameter] public string CashierName { get; set; }
    [Parameter] public string OrganizationEnglishName { get; set; }

}

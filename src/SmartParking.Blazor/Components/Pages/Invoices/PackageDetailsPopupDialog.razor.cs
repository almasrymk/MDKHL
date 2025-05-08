using Microsoft.AspNetCore.Components;
using SmartParking.Packages;

namespace SmartParking.Blazor.Components.Pages.Invoices;

public partial class PackageDetailsPopupDialog
{
    [Parameter] public PackageDto? Package { get; set; }
    [Parameter] public string OrganizationAddress { get; set; }

}

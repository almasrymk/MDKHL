using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups;

namespace SmartParking.Blazor.Components.Pages.PackageTypes;

public partial class DetailsPackageTypeDialog
{

    [CascadingParameter] IMudDialogInstance MudDialog { get; set; }
    [Parameter] public PackageTypeDto packageType { get; set; } = new PackageTypeDto();


    private void Cancel()
    {
        MudDialog.Cancel();
    }
}

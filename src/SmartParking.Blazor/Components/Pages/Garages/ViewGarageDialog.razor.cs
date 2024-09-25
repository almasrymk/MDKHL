using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MudBlazor;
using SmartParking.Garages;
using SmartParking.OrganizationsGarages;

namespace SmartParking.Blazor.Components.Pages.Garages;

public partial class ViewGarageDialog
{

    [CascadingParameter]
    private MudDialogInstance MudDialogAdd { get; set; }
    [Parameter] public GarageDto Garage { get; set; }

    private void OnCloseViewOnly()
    {
        MudDialogAdd.Cancel();

    }
}

using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups.VehicleModel;
namespace SmartParking.Blazor.Components.Pages.VehicleModels;
public partial class ViewVehicleModelDialog
{
    [CascadingParameter]
    private IMudDialogInstance MudDialogAdd { get; set; }
    [Parameter] public CreateUpdateVehicleModelDto VehicleModel { get; set; }

    bool _clearable;
    private MudForm form;

    protected override async Task OnInitializedAsync()
    {

        await base.OnInitializedAsync();
    }
    private void OnCloseViewOnly()
    {
        MudDialogAdd.Cancel();
    }

}

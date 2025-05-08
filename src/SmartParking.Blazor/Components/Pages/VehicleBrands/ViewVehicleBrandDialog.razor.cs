using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups.VehicleBrand;

namespace SmartParking.Blazor.Components.Pages.VehicleBrands
{
    partial class ViewVehicleBrandDialog
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialogAdd { get; set; }
        [Parameter] public CreateUpdateVehicleBrandDto VehicleBrand { get; set; }

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
}

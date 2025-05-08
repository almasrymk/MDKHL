using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups.CameraBrands;

namespace SmartParking.Blazor.Components.Pages.CameraBrand
{
    partial class ViewCameraBrandDialog
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialogAdd { get; set; }
        [Parameter] public CameraBrandDto CameraBrand { get; set; }

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

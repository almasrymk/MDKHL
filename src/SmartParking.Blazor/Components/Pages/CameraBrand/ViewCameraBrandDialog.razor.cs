using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups.CameraBrands;

namespace SmartParking.Blazor.Components.Pages.CameraBrand
{
    /// <summary>
    /// Partial class responsible for handling the view logic of a Camera Brand using a dialog UI.
    /// </summary>
    partial class ViewCameraBrandDialog
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialogAdd { get; set; }
        [Parameter] public CameraBrandDto CameraBrand { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }
        /// <summary>
        /// Closes the view dialog.
        /// </summary>
        private void OnCloseViewOnly()
        {
            MudDialogAdd.Cancel();
        }

    }
}

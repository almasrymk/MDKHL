using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Shifts;

namespace SmartParking.Blazor.Components.Pages.Shifts
{
    partial class ShiftViewDialog
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; }
        [Parameter]
        public ShiftDto shiftDto { get; set; }

        private void OnCloseViewOnly()
        {
            MudDialog.Cancel();

        }
    }
}

using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.KeeperShifts;

namespace SmartParking.Blazor.Components.Pages.MyShifts
{
    partial class ViewMyShifts
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialogAdd { get; set; }
        [Parameter] public KeeperShiftDto KeeperShiftDto { get; set; }
        private MudForm form;
        protected override Task OnInitializedAsync()
        {
            return base.OnInitializedAsync();
        }

        private void OnCloseViewOnly()
        {
            MudDialogAdd.Cancel();
        }
    }
}

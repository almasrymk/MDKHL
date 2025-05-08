using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.DailyShifts;

namespace SmartParking.Blazor.Components.Pages.DailyShift
{
    partial class ViewDailyShift
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialogAdd { get; set; }
        [Parameter] public DailyShiftDto DailyShiftDto { get; set; }
        private MudForm form;
        string KeeperShiftRequestType = "";

        protected override Task OnInitializedAsync()
        {
            KeeperShiftRequestType = DailyShiftDto.KeeperShiftRequestType.ToString();
            return base.OnInitializedAsync();
        }

        private void OnCloseViewOnly()
        {
            MudDialogAdd.Cancel();

        }
    }
}

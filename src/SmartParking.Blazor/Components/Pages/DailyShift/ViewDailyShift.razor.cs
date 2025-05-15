using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.DailyShifts;

namespace SmartParking.Blazor.Components.Pages.DailyShift
{
    /// <summary>
    /// component for view daily shift
    /// </summary>
    partial class ViewDailyShift
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialogAdd { get; set; }
        [Parameter] public DailyShiftDto DailyShiftDto { get; set; }
        private MudForm form;
        string KeeperShiftRequestType = "";
        /// <summary>
        /// Initializes the component.
        /// </summary>
        protected override Task OnInitializedAsync()
        {
            KeeperShiftRequestType = DailyShiftDto.KeeperShiftRequestType.ToString();
            return base.OnInitializedAsync();
        }
        /// <summary>
        /// Closes the view  dialog.
        /// </summary>
        private void OnCloseViewOnly()
        {
            MudDialogAdd.Cancel();

        }
    }
}

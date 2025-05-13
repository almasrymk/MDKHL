using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Cameras;

namespace SmartParking.Blazor.Components.Pages.Cameras
{
    /// <summary>
    /// Partial class for viewing camera details in a read-only dialog.
    /// </summary>
    partial class ViewCameraDialog
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialogAdd { get; set; }
        [Parameter]
        public CameraDto Camera { get; set; }
        [Parameter]
        public int UpdatedId { get; set; }

        private bool _clearable;
        private MudForm form;

        /// <summary>
        /// Closes the view-only dialog.
        /// </summary>
        private void OnCloseViewOnly()
        {
            MudDialogAdd.Cancel();
        }
    }

}

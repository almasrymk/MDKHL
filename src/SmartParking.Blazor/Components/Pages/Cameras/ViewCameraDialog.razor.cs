using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Cameras;

namespace SmartParking.Blazor.Components.Pages.Cameras
{
    partial class ViewCameraDialog
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialogAdd { get; set; }
        [Parameter] public CameraDto Camera { get; set; }
        [Parameter]
        public int UpdatedId { get; set; }
        bool _clearable;
        private MudForm form;

        private void OnCloseViewOnly()
        {
            MudDialogAdd.Cancel();

        }
    }
}

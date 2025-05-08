using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups.CameraModel;

namespace SmartParking.Blazor.Components.Pages.CameraModel
{
    partial class ViewCameraModelDialog
    {

        private MudForm form;
        [Parameter]
        public CameraModelDto cameraModel { get; set; } = new CameraModelDto();
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; }
        private void Cancel()
        {
            MudDialog.Cancel();
        }
    }
}

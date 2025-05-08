using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups.CameraMainType;

namespace SmartParking.Blazor.Components.Pages.CameraMainType
{
    partial class ViewCameraMainTypeDialog
    {
        [Parameter]
        public CameraMainTypeDto cameraMainType { get; set; } = new CameraMainTypeDto();
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; }
        private void Cancel()
        {
            MudDialog.Cancel();
        }
    }
}

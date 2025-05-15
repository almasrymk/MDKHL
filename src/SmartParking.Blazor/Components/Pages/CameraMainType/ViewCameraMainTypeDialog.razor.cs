using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups.CameraMainType;

namespace SmartParking.Blazor.Components.Pages.CameraMainType
{
/// <summary>
/// Dialog component for viewing an existing camera main type.
/// </summary>
partial class ViewCameraMainTypeDialog
    {
        /// <summary>
        /// The camera main type to be displayed in the dialog.
        /// </summary>
        [Parameter]
        public CameraMainTypeDto cameraMainType { get; set; } = new CameraMainTypeDto();
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; }
        /// <summary>
        /// Close the dialog
        /// </summary>
        private void Cancel()
        {
            
            MudDialog.Cancel();
        }
    }
}

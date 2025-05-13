using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups.CameraModel;

namespace SmartParking.Blazor.Components.Pages.CameraModel;

/// <summary>
/// Component for viewing an existing camera model using a dialog form.
/// </summary>
partial class ViewCameraModelDialog
{
    [Parameter]
    public CameraModelDto cameraModel { get; set; } = new CameraModelDto();

    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }
    private MudForm form;
    private bool _clearable;

    /// <summary>
    /// Cancels the dialog without saving any changes.
    /// </summary>
    private void Cancel()
    {
        MudDialog.Cancel();
    }
}


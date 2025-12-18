using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.JSInterop;
using MudBlazor;
using SmartParking.Cameras;
using SmartParking.Invoices;

namespace SmartParking.Blazor.Components.Pages.Cameras;

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
    private bool ShowQrDownload { get; set; } = false;


    /// <summary>
    /// Closes the view-only dialog.
    /// </summary>
    private void OnCloseViewOnly()
    {
        MudDialogAdd.Cancel();
    }

    private async Task HandleDownload()
    {
        string watcherCode = Camera.DeviceId;
        if (string.IsNullOrEmpty(watcherCode))
            return;
        var uri = new Uri(Navigation.BaseUri);
        string domain = $"{uri.Scheme}://{uri.Host}{(uri.IsDefaultPort ? "" : ":" + uri.Port)}/";
        var qr = await InvoiceAppService.QrCodeForInvoicePage((string)watcherCode, (string)domain);
        await JSRuntime.InvokeVoidAsync("window.open", qr, "_blank");
    }
}

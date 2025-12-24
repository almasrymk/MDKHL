using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.JSInterop;
using MudBlazor;
using SmartParking.Cameras;
using SmartParking.Cameras.Watchers.Dispensers;
using SmartParking.Garages;
using SmartParking.Invoices;
using SmartParking.Organizations;

namespace SmartParking.Blazor.Components.Pages.Cameras.Dispensers;

public partial class ViewDispenser
{
    [CascadingParameter]
    private IMudDialogInstance MudDialogAdd { get; set; }
    [Parameter]
    public DispenserDto Dispenser { get; set; }
    [Parameter]
    public int UpdatedId { get; set; }

    private bool _clearable;
    private MudForm form;
    OrganizationDto organization = new OrganizationDto();
    GarageDto garage = new GarageDto();

    protected override async Task OnInitializedAsync()
    {
        garage = await GaragesAppService.GetAsync(Dispenser.GarageId);
         organization = await OrganizationAppService.GetAsync(Dispenser.OrganizationId);
        await base.OnInitializedAsync();
    }
    /// <summary>
    /// Closes the view-only dialog.
    /// </summary>
    private void OnCloseViewOnly()
    {
        MudDialogAdd.Cancel();
    }

    private async Task HandleDownload()
    {
        string watcherCode = Dispenser.DeviceId;
        if (string.IsNullOrEmpty(watcherCode))
            return;
        var uri = new Uri(Navigation.BaseUri);
        string domain = $"{uri.Scheme}://{uri.Host}{(uri.IsDefaultPort ? "" : ":" + uri.Port)}/";
        var qr = await InvoiceAppService.QrCodeForInvoicePage((string)watcherCode, (string)domain);
        await JSRuntime.InvokeVoidAsync("window.open", qr, "_blank");
    }
}

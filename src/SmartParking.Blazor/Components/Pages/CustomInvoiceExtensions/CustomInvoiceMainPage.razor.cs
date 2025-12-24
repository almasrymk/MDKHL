using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Pages.Cameras;
using SmartParking.Entities.Cameras.Watchers;
using SmartParking.Enums;
using SmartParking.Invoices;
using SmartParking.Payments;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Blazor.Components.Pages.CustomInvoiceExtensions;

public partial class CustomInvoiceMainPage
{
    [Parameter] public string WatcherCode { get; set; }
    private string SearchText { get; set; }
    private string TicketSearchText { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; }
    public DateTime InvoiceDate { get; set; }
    public double InvoicePrice { get; set; }
    public double Vat { get; set; }
    public DateTime Checkout { get; set; }
    public DateTime CheckIn { get; set; }
    public string Duration { get; set; }
    public string EntryPicture { get; set; }
    public string ExitPicture { get; set; }
    public string PlateNumber { get; set; }
    public double InvoiceAmount { get; set; }
    public int VatRate { get; set; }
    private string paymentMethod { get; set; } = "Mada";
    private InvoiceDto? LatestInvoice { get; set; }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var invoice = await invoiceAppService
            .GetLatestInvoiceForOrganizationByWatcher(WatcherCode);
            InvoiceNumber = invoice.InvoiceId.ToString();
            InvoiceDate = invoice.InvoiceTime!.Value;
            InvoicePrice = (double)invoice.InvoicePrice;
            Vat = (double)invoice.Vat;

            var duration = invoice.InvoiceTime.Value - invoice.EntranceTime.Value;
            Duration = $"{(int)duration.TotalHours} hours {duration.Minutes} minutes";

            CheckIn = invoice.EntranceTime.Value;
            Checkout = invoice.InvoiceTime.Value;

            EntryPicture = invoice.EntryTransactionImage;
            ExitPicture = invoice.ExitTransactionImage;
            PlateNumber = invoice.PlateNumber;

            InvoiceAmount = InvoicePrice + Vat;
            VatRate = (int)(Vat / InvoicePrice * 100);
        }
        catch (Exception ex)
        {
            navigationManager.NavigateTo("/", true);
        }

    }

    private void OnPaymentMethodChanged(string method)
    {
        paymentMethod = method;
    }

    private async Task HandleCheckout()
    {
        var dto = new PayCustomerInvoiceDto
        {
            InvoiceId = int.Parse(InvoiceNumber),
            Method = Enum.TryParse<PaymentMethod>(paymentMethod, true, out var methodEnum) ? methodEnum : PaymentMethod.MADA
        };
        try
        {
            await paymentAppService.PayCustomerInvoiceAsync(dto);
            snackbar.Add(L["PaymentSuccess"], Severity.Success);


        }
        catch (Exception ex)
        {
            snackbar.Add(L["PaymentFailed"], Severity.Error);
        }
        await Task.Delay(500);
        navigationManager.NavigateTo("/", true);
    }

    private async Task PlateNumberSearch(Watcher watcher) {
        LatestInvoice = await Invoive.GetLatestInvoiceForCustomerInOrganizationGarage(SearchText, watcher.OrganizationId, watcher.GarageId);
        if (LatestInvoice != null)
        {
            InvoiceNumber = LatestInvoice.InvoiceId.ToString();
            InvoiceDate = LatestInvoice.InvoiceTime!.Value;
            InvoicePrice = (double)LatestInvoice.InvoicePrice;
            Vat = (double)LatestInvoice.Vat;

            var duration = LatestInvoice.InvoiceTime.Value - LatestInvoice.EntranceTime.Value;
            Duration = $"{(int)duration.TotalHours} hours {duration.Minutes} minutes";

            CheckIn = LatestInvoice.EntranceTime.Value;
            Checkout = LatestInvoice.InvoiceTime.Value;

            EntryPicture = LatestInvoice.EntryTransactionImage;
            ExitPicture = LatestInvoice.ExitTransactionImage;
            PlateNumber = LatestInvoice.PlateNumber;

            InvoiceAmount = InvoicePrice + Vat;
            VatRate = (int)(Vat / InvoicePrice * 100);
        }

    }
    private async Task OnSearch()
    {
        //var camera = await watcherAppService.GetcameraByWatherCodeAsync(WatcherCode);
        //if (camera != null)
        //{
        //    await PlateNumberSearch(camera);
        //}
        //else
        //{
        //    var dispenser = await watcherAppService.GetDispenserByWatherCodeAsync(WatcherCode);
        //    if (dispenser != null)
        //    {
        //        await PlateNumberSearch(dispenser);
        //    }
        //}
        

        var dto = new InvoiceSearchDto();
        dto.PlateNumber = SearchText;
        dto.TicketId = TicketSearchText;

        var watcher = await InvoiceActionsFactory.GetWatcherByCodeAsync(dto, WatcherCode);
        await PlateNumberSearch(watcher);
        StateHasChanged();
    }

    private void OnSearchValueChanged(string searchValue)
    {
        SearchText = searchValue;
        StateHasChanged();
    }

}

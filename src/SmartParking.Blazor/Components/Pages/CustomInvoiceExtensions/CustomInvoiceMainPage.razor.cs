using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Pages.Cameras;
using SmartParking.Blazor.Dtos;
using SmartParking.Entities.Cameras.Watchers;
using SmartParking.Enums;
using SmartParking.Invoices;
using SmartParking.Payments;
using SmartParking.Validations;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Blazor.Components.Pages.CustomInvoiceExtensions;

public partial class CustomInvoiceMainPage
{
    [Parameter] public string WatcherCode { get; set; }
    private string SearchText { get; set; }
    private string PlateSearchText { get; set; } = string.Empty;
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

    private CreateUpdateInvoiceUiDto? invoiceDto { get; set; } = new();

    private bool ShowInvoiceDetails = false;

    private InvoiceSearchDto InvoiceSearchDto = new InvoiceSearchDto();


    protected override async Task OnInitializedAsync()
    {
        try
        {
            InvoicePrice = 0;
            VatRate = 15;
            Vat = 0;
            InvoiceAmount = 0;
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
        var invoice = await invoiceUiAppService.GetInvoiceAfterCheckout(InvoiceSearchDto);
        InvoiceNumber = invoice.InvoiceNumber;
        var dto = new PayCustomerInvoiceDto
        {
            InvoiceId = invoice.Id,
            Method = Enum.TryParse<PaymentMethod>(paymentMethod, true, out var methodEnum) ? methodEnum : PaymentMethod.MADA
        };
        try
        {
            var paymentInvoice = await paymentAppService.PayCustomerInvoiceAsync(dto);
            snackbar.Add(L["PaymentSuccess"], Severity.Success);
            await Task.Delay(1000);
            navigationManager.NavigateTo($"/custom-invoice/details/{paymentInvoice.Value.PaymentId}");

        }
        catch (Exception ex)
        {
            snackbar.Add(L["PaymentFailed"], Severity.Error);
        }
        //await Task.Delay(500);
        //navigationManager.NavigateTo("/", true);
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


        //var dto = new InvoiceSearchDto();
        //string te = VehicleValidation.MapToEnglishNumerals(SearchText);
        InvoiceSearchDto.TicketId = VehicleValidation.HandleKeyUp(SearchText).Item1;
        InvoiceSearchDto.PlateNumber = PlateSearchText;

        invoiceDto = await invoiceUiAppService.GetInvoiceAfterPreCheckout(InvoiceSearchDto);
        if (invoiceDto is not null)
        {
            ShowInvoiceDetails = true;
            InvoiceNumber = invoiceDto.InvoiceNumber;
            InvoiceDate = invoiceDto.InvoiceTime!.Value;
            InvoicePrice = (double)invoiceDto.InvoicePrice;
            Vat = (double)invoiceDto.Vat;
            var duration = invoiceDto.InvoiceTime.Value - invoiceDto.EntranceTime.Value;
            Duration = $"{(int)duration.TotalHours} hours {duration.Minutes} minutes";
            CheckIn = invoiceDto.EntranceTime.Value;
            Checkout = invoiceDto.InvoiceTime.Value;
            EntryPicture = invoiceDto.EntryTransactionImage;
            ExitPicture = invoiceDto.ExitTransactionImage;
            PlateNumber = invoiceDto.PlateNumber;
            InvoiceAmount = InvoicePrice + Vat;
            VatRate = 15;
        }
        StateHasChanged();
    }

    private void OnSearchValueChanged(string searchValue)
    {
        SearchText = searchValue;
        StateHasChanged();
    }

}

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using SmartParking.Blazor.Dtos;
using SmartParking.Enums;
using SmartParking.Invoices;
using SmartParking.Payments;
using SmartParking.Payments.PaymentIntegrations;
using SmartParking.Results;
using SmartParking.Utilities;
using SmartParking.Validations;

namespace SmartParking.Blazor.Components.Pages.CustomInvoiceExtensions;

public partial class CustomInvoiceMainPage
{
    [Parameter] public string WatcherCode { get; set; }
    private string SearchText { get; set; }
    private string PlateSearchText { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; }
    public DateTime InvoiceDate { get; set; }
    public decimal InvoicePrice { get; set; }
    public decimal Vat { get; set; }
    public DateTime Checkout { get; set; }
    public DateTime CheckIn { get; set; }
    public string Duration { get; set; }
    public string EntryPicture { get; set; }
    public string ExitPicture { get; set; }
    public string PlateNumber { get; set; }
    public decimal InvoiceAmount { get; set; }
    public int VatRate { get; set; }
    private string paymentMethod { get; set; } = "MADA";
    private InvoiceDto? LatestInvoice { get; set; }

    private CreateUpdateInvoiceUiDto? invoiceDto { get; set; } = new();

    private bool ShowInvoiceDetails = false;
    private PaymentMethod paymentMethodValue { get; set; } = PaymentMethod.MADA;

    private InvoiceSearchDto InvoiceSearchDto = new InvoiceSearchDto();
    private static readonly Dictionary<string,PaymentMethod> PaymentMap =
    new()
    {
        { "MADA" ,PaymentMethod.MADA },
        { "APPLEPAY" , PaymentMethod.ApplePay }
    };


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
        if (!PaymentMap.TryGetValue(method, out var brand))
            return;
        paymentMethodValue = brand;

    }
    private async Task ProceedToPayment()
    {
        try
        {
            // 🔹 Normally this comes from backend (Create Checkout API)
            var invoice = await invoiceUiAppService.GetInvoiceAfterCheckout(InvoiceSearchDto);
            var checkoutId = await OppwaIntegrationAppService.PrepareCheckoutAsync((decimal)InvoiceAmount, paymentMethodValue);

            await OppwaIntegrationAppService.SavePaymentRequest(new PaymentCheckoutRequestDto
            {
                InvoiceId = invoice.InvoiceId,
                OrganizationId = invoice.OrganizationId,
                Amount = PaymentUtilityCalculationts.GetPriceForPayment(invoice.InvoicePrice, invoice.Discount),
                PaymentCategory = PaymentCategory.PaymentParking,
                PaymentMethod = paymentMethodValue,
                TenantId = invoice.TenantId.Value,
                EnPlateNumber = invoice.PlateNumber,
            }, checkoutId);

            await JS.InvokeVoidAsync(
                "hyperPay.loadWidget",
                checkoutId,
                $"{navigationManager.BaseUri}api/PaymentResult/hyperpay/result",
                paymentMethodValue.ToString()
            );
        }
        catch (Exception ex)
        {
            snackbar.Add(L["PaymentFailed"], Severity.Error);
        }
    }

    private async Task OnSearch()
    {

        try
        {
            InvoiceSearchDto.TicketId = VehicleValidation.HandleKeyUp(SearchText).Item1;
            if (SearchText.Length != 11)
            {
                snackbar.Add(L["InvalidTicket"],Severity.Warning);
                return;
            }
            InvoiceSearchDto.PlateNumber = PlateSearchText;

            invoiceDto = await invoiceUiAppService.GetInvoiceAfterPreCheckout(InvoiceSearchDto);
            if (invoiceDto is not null)
            {
                ShowInvoiceDetails = true;
                InvoiceNumber = invoiceDto.InvoiceNumber;
                InvoiceDate = invoiceDto.InvoiceTime!.Value;
                InvoicePrice = Math.Round(invoiceDto.InvoicePrice,2);
                Vat = Math.Round(invoiceDto.Vat,2);
                var duration = invoiceDto.InvoiceTime.Value - invoiceDto.EntranceTime.Value;
                Duration = $"{(int)duration.TotalHours} hours {duration.Minutes} minutes";
                CheckIn = invoiceDto.EntranceTime.Value;
                Checkout = invoiceDto.InvoiceTime.Value;
                EntryPicture = invoiceDto.EntryTransactionImage;
                ExitPicture = invoiceDto.ExitTransactionImage;
                PlateNumber = invoiceDto.PlateNumber;
                InvoiceAmount = Math.Round(InvoicePrice + Vat,2);
                VatRate = 15;
            }
            else
            {
                snackbar.Add(L["InvoiceNotFound"], Severity.Error);
                await Task.Delay(1000);
            }
        }
        catch (Exception ex) {
            snackbar.Add(L["InvoiceNotFound"], Severity.Error);
            await Task.Delay(1000);
        }
        StateHasChanged();
    }

    private void OnSearchValueChanged(string searchValue)
    {
        SearchText = searchValue;
        StateHasChanged();
    }

}

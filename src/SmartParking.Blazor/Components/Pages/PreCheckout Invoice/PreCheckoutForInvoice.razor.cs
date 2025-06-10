using Microsoft.AspNetCore.Components;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Enums;
using SmartParking.GarageTransactions;
using SmartParking.Invoices;
using SmartParking.Payments;
using SmartParking.Utilities;

namespace SmartParking.Blazor.Components.Pages.PreCheckout_Invoice;

public partial class PreCheckoutForInvoice
{
    [Parameter]
    public int GarageTransactionId { get; set; }

    public CreateUpdateInvoiceDto Invoice { get; set; } = new();
    public GarageTransactionDto GarageTransactionDto { get; set; } = new();
    public int Id { get; set; }
    private bool NoInvoice => Invoice == null || string.IsNullOrEmpty(Invoice?.PlateNumber);
    public decimal Discount { get; set; } = default;
    private PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    private void Cancel()
    {
        Navigation.NavigateTo("/garagetransactions");
    }

    private async Task PayInvoice()
    {
        var invoice = await keeperTransactionsAppService.Checkout(GarageTransactionId);
        Id = invoice.Value.Id;
        if (Discount != default)
        {
            await InvoiceAppService.UpdateDiscountForInvoice(Id, Discount);
        }
        PaymentCheckoutDto payment = await RequestToPaymentCheckout();
        if (payment != null)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]}  {L["Checkout"]}", MudBlazor.Severity.Success);
            await Task.Delay(1000);
            Navigation.NavigateTo($"/payments/view/{payment.PaymentId}");
        }
    }

    private async Task<PaymentCheckoutDto?> RequestToPaymentCheckout()
    {

        var paymentCheckout = new PaymentCheckoutRequestDto
        {
            OrganizationId = Invoice.OrganizationId,
            PaymentCategory = PaymentCategory.PaymentParking,
            InvoiceId = Id,
            PaymentMethod = PaymentMethod,
            Amount = PaymentUtilityCalculationts.GetPriceForPayment(Invoice.InvoicePrice, Discount),
            CustomerId = Invoice.CustomerId
        };

        return await PaymentAppSerivce.CreateCheckoutInvoicePaymentWebAsync(paymentCheckout);
    }


    protected override async Task OnInitializedAsync()
    {
        GarageTransactionDto = await GarageTransactionAppService.GetAsync(GarageTransactionId);
        Invoice = await keeperTransactionsAppService.PreCheckout(GarageTransactionId);
    }
}

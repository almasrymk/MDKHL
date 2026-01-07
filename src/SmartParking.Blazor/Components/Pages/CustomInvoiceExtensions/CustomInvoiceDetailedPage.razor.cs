using Microsoft.AspNetCore.Components;
using SmartParking.Blazor.Dtos;

namespace SmartParking.Blazor.Components.Pages.CustomInvoiceExtensions;

public partial class CustomInvoiceDetailedPage
{
    [Parameter] public string PaymentId { get; set; }
    private decimal subtotal { get; set; }
    private InvoiceCheckoutUiDto invoiceCheckout { get; set; } = new();
    protected override async Task OnInitializedAsync()
    {
        invoiceCheckout = await invoiceCheckoutUiAppService.GetInvoiceCheckoutAsync(Guid.Parse(PaymentId));
        subtotal = (invoiceCheckout.InvoicePrice + invoiceCheckout.Discount - (decimal)invoiceCheckout.Vat) ;
    }
}

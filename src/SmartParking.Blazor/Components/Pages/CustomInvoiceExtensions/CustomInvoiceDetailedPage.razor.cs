using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SmartParking.Blazor.Dtos;
using System.Drawing.Printing;
using static QRCoder.PayloadGenerator;

namespace SmartParking.Blazor.Components.Pages.CustomInvoiceExtensions;

public partial class CustomInvoiceDetailedPage
{
    [Parameter] public string PaymentId { get; set; }
    [Parameter] public RenderFragment ChildContent { get; set; }

    private decimal subtotal { get; set; }
    private InvoiceCheckoutUiDto invoiceCheckout { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        invoiceCheckout = await invoiceCheckoutUiAppService.GetInvoiceCheckoutAsync(Guid.Parse(PaymentId));
        subtotal = (invoiceCheckout.InvoicePrice + invoiceCheckout.Discount - (decimal)invoiceCheckout.Vat) ;
    }

    public async Task Print()
    {
        var title = "Invoice " + invoiceCheckout.InvoiceNumber;
        var text = $"Total: {invoiceCheckout.InvoicePrice} SAR";
        var url = NavigationManager.Uri;

        await JS.InvokeVoidAsync(
            "openShare",
            title,
            text,
            url
        );
    }
}

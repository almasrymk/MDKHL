using Microsoft.AspNetCore.Components;

namespace SmartParking.Blazor.Components.Pages.Invoices;

public partial class PreInvoiceSummaryComponent
{
    [Parameter] public bool IsPaid { get; set; }
    [Parameter] public decimal? InvoicePrice { get; set; }
    [Parameter] public EventCallback PayInvoice { get; set; }
    [Parameter] public EventCallback Cancel { get; set; }
    [Parameter] public int ComponentSize { get; set; }

    public decimal InvoicePriceWithVat { get; set; }
    public decimal VAT => Math.Round((InvoicePrice ?? 0) * 0.15m, 2);

    protected override void OnParametersSet()
    {
        InvoicePriceWithVat = Math.Round((InvoicePrice ?? 0) + VAT, 2);
    }
}

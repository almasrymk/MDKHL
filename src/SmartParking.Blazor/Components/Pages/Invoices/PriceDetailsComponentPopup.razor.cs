using Microsoft.AspNetCore.Components;

namespace SmartParking.Blazor.Components.Pages.Invoices;

public partial  class PriceDetailsComponentPopup
{
    [Parameter] public string Total { get; set; }
    [Parameter] public string Amount { get; set; }
    [Parameter] public string Discount { get; set; } = default;
    [Parameter] public string Vat { get; set; }
}

using Microsoft.AspNetCore.Components;

namespace SmartParking.Blazor.Components.Pages.LastInvoiceAtCamera;

public partial  class LastInvoiceOnGateDetails
{
    [Parameter] public MarkupString CurrencyKey { get; set; }
    [Parameter] public string Key { get; set; }
    [Parameter] public string Value { get; set; }
}

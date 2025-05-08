using Microsoft.AspNetCore.Components;

namespace SmartParking.Blazor.Components.Pages.Invoices;

public partial  class PriceDetailsComponentPopup
{
    [Parameter] public Decimal Price { get; set; }
    [Parameter] public Decimal PaymentPrice { get; set; }
    [Parameter] public bool HasDiscount { get; set; }
}

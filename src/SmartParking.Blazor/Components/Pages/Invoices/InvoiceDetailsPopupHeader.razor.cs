using Microsoft.AspNetCore.Components;

namespace SmartParking.Blazor.Components.Pages.Invoices;

public partial class InvoiceDetailsPopupHeader
{
    [Parameter] public string Title { get; set; }
    [Parameter] public string Description { get; set; }
}

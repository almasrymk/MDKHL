using Microsoft.AspNetCore.Components;
using SmartParking.Invoices;

namespace SmartParking.Blazor.Components.Pages.Invoices;

public partial class InvoiceDetailsPopupDialog
{
    [Parameter] public string OrganizationAddress { get; set; }
    [Parameter] public InvoiceDto? Invoice { get; set; }
}

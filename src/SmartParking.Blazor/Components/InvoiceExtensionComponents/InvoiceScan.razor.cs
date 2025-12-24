using Microsoft.AspNetCore.Components;

namespace SmartParking.Blazor.Components.InvoiceExtensionComponents;

public partial class InvoiceScan
{
    [Parameter]
    public string SearchText { get; set; }

    [Parameter]
    public EventCallback<string> OnSearchTextChanged { get; set; }

    [Parameter]
    public EventCallback OnButtonTriggered { get; set; }

}

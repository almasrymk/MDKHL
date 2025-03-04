using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Discounts;
using SmartParking.Lookups;

namespace SmartParking.Blazor.Components.Pages.Discount;

public partial class DetailsDiscountDialog
{


    [CascadingParameter] MudDialogInstance MudDialog { get; set; }
    [Parameter] public DiscountDto discount { get; set; } = new DiscountDto();


    private void Cancel()
    {
        MudDialog.Cancel();
    }
}

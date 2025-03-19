using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Discounts;

namespace SmartParking.Blazor.Components.Pages.Discount;

public partial class DetailsDiscountDialog
{


    [CascadingParameter] IMudDialogInstance MudDialog { get; set; }
    [Parameter] public DiscountDto discount { get; set; } = new DiscountDto();


    private void Cancel()
    {
        MudDialog.Cancel();
    }
}

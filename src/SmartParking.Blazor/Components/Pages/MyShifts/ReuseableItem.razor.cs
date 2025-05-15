using Microsoft.AspNetCore.Components;

namespace SmartParking.Blazor.Components.Pages.MyShifts
{
    partial class ReuseableItem
    {
        [Parameter] public string Value { get; set; }
        [Parameter] public string Localize { get; set; }
    }
}

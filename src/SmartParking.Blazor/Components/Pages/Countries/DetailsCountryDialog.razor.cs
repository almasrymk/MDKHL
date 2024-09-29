using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups;

namespace SmartParking.Blazor.Components.Pages.Countries;

public partial class DetailsCountryDialog
{


    [CascadingParameter] MudDialogInstance MudDialog { get; set; }
    [Parameter] public CountryDto country { get; set; } = new CountryDto();


    private void Cancel()
    {
        MudDialog.Cancel();
    }
}

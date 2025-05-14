using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups;

namespace SmartParking.Blazor.Components.Pages.Countries;

/// <summary>
/// Represents the dialog for displaying the details of a country.
/// This dialog allows the user to view the details of a selected country.
/// </summary>
public partial class DetailsCountryDialog
{
    [CascadingParameter]
    IMudDialogInstance MudDialog { get; set; }
    [Parameter]
    public CountryDto country { get; set; } = new CountryDto();

    /// <summary>
    /// Closes the dialog without performing any action.
    /// This method is typically used to cancel the action and close the dialog.
    /// </summary>
    private void Cancel()
    {
        MudDialog.Cancel();
    }
}

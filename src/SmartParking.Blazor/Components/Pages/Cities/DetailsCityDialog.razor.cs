using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups;
using SmartParking.Lookups.State;

namespace SmartParking.Blazor.Components.Pages.Cities;

/// <summary>
/// Represents the dialog component used for displaying details of a city.
/// </summary>
public partial class DetailsCityDialog
{

    [CascadingParameter]
    IMudDialogInstance MudDialog { get; set; }
    [Parameter]
    public CityDto city { get; set; } = new CityDto();
    public IReadOnlyList<StateLookupDto> states { get; set; } = new List<StateLookupDto>();

    /// <summary>
    /// Called when the component is initialized. Loads the list of state lookups.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected async override Task OnInitializedAsync()
    {
        var result = await StateAppService.GetStateLookupAsync();
        states = result;
    }
    /// <summary>
    /// Closes the dialog without saving changes.
    /// </summary>
    private void Cancel()
    {
        MudDialog.Cancel();
    }
}

using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups;
using SmartParking.Lookups.State;

namespace SmartParking.Blazor.Components.Pages.Cities;

public partial class DetailsCityDialog
{

    [CascadingParameter] IMudDialogInstance MudDialog { get; set; }
    [Parameter] public CityDto city { get; set; } = new CityDto();
    public IReadOnlyList<StateLookupDto> states { get; set; } = new List<StateLookupDto>();

    protected async override Task OnInitializedAsync()
    {
        var result = await StateAppService.GetStateLookupAsync();
        states = result;
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }
}

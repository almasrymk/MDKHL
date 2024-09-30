using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups;
using SmartParking.Lookups.City;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartParking.Blazor.Components.Pages.Cities;

public partial class DetailsCityDialog
{

    [CascadingParameter] MudDialogInstance MudDialog { get; set; }
    [Parameter] public CityDto city { get; set; } = new CityDto();
    public IReadOnlyList<GovernorateLookupDto> governorates { get; set; } = new List<GovernorateLookupDto>();

    protected async override Task OnInitializedAsync()
    {
        var result = await CitiesAppService.GetGovernorateLookupAsync();
        governorates = result;
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }
}

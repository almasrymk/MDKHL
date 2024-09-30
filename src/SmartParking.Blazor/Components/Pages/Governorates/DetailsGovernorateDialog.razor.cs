using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups.Governorate;
using SmartParking.Lookups;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartParking.Blazor.Components.Pages.Governorates;

public partial class DetailsGovernorateDialog
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; }
    [Parameter] public GovernorateDto governorate { get; set; } = new GovernorateDto();
    public IReadOnlyList<CountryLookupDto> countries { get; set; } = new List<CountryLookupDto>();

    bool _clearable;

    protected override async Task OnInitializedAsync()
    {
        var result = await GovernorateAppService.GetCountryLookupAsync();
        countries = result;
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }
}

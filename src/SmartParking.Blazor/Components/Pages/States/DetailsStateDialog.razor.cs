using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups.State;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartParking.Blazor.Components.Pages.States;

public partial class DetailsStateDialog
{
    [CascadingParameter] MudDialogInstance MudDialog { get; set; }
    [Parameter] public StateDto state { get; set; } = new StateDto();
    public IReadOnlyList<RegionLookupDto> regions { get; set; } = new List<RegionLookupDto>();


    protected override async Task OnInitializedAsync()
    {
        var result = await StateAppService.GetRegionLookupAsync();
        regions = result;
    }




    private void Cancel()
    {
        MudDialog.Cancel();
    }
}

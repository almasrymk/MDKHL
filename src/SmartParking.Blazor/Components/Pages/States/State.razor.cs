using MudBlazor;
using SmartParking.Blazor.Components.Pages.Cities;
using SmartParking.Lookups;
using SmartParking.Lookups.State;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Blazor.Components.Pages.States;

public partial class State
{
    private IReadOnlyList<StateDto> states = new List<StateDto>();
    private int currentPage = 1;
    private int pageSize = 100;

    private readonly DialogOptions _backdropClick = new() { BackdropClick = false };


    protected override async Task OnInitializedAsync()
    {

        await base.OnInitializedAsync();
        await GetData();
    }

    private async Task GetData()
    {
        var result = await AppService.GetListAsync(new PagedAndSortedResultRequestDto
        {
            MaxResultCount = pageSize,
            SkipCount = (currentPage - 1) * pageSize
        });

        states = result.Items.ToList();
        StateHasChanged();
    }
    private Task OpenCreateStateDialog(DialogOptions options)
    {
        return Dialog.ShowAsync<AddCityDialog>("Add City", options);
    }

    private async Task OpenEditStateDialog(StateDto stateDto)
    {
        var parameters = new DialogParameters { ["State"] = stateDto };
        var dialog = await Dialog.ShowAsync<EditCityDialog>("Edit State", parameters);
        var result = await dialog.Result;


    }

    private async Task OpenDetailsStateDialog(StateDto stateDto)
    {
        var parameters = new DialogParameters { ["State"] = stateDto };
        var dialog = await Dialog.ShowAsync<DetailsCityDialog>("State Details", parameters);
        var result = await dialog.Result;


    }
}

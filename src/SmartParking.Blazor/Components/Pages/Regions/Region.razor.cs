using MudBlazor;
using SmartParking.Lookups.Region;
using SmartParking.Lookups.State;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Blazor.Components.Pages.Regions;

public partial class Region
{
    private IReadOnlyList<RegionDto> regions = new List<RegionDto>();
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

        regions = result.Items.ToList();
        StateHasChanged();
    }
    private Task OpenCreateRegionDialog(DialogOptions options)
    {
        return Dialog.ShowAsync<AddRegionDialog>("Add Region", options);
    }

    private async Task OpenEditRegionDialog(RegionDto regionDto)
    {
        var parameters = new DialogParameters { ["Region"] = regionDto };
        var dialog = await Dialog.ShowAsync<EditRegionDialog>("Edit Region", parameters);
        var result = await dialog.Result;


    }

    private async Task OpenDetailsRegionDialog(RegionDto region)
    {
        var parameters = new DialogParameters { ["Region"] = region };
        var dialog = await Dialog.ShowAsync<DetailsRegionDialog>("Region Details", parameters);
        var result = await dialog.Result;


    }
}

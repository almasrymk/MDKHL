using MudBlazor;
using SmartParking.Blazor.Components.Pages.Cities;
using SmartParking.Lookups;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Blazor.Components.Pages.PackageTypes;

public partial class PackageType
{
    private IReadOnlyList<PackageTypeDto> packageTypes = new List<PackageTypeDto>();
    private int currentPage = 1;
    private int pageSize = 100;
    //Dialog

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

        packageTypes = result.Items.ToList();
        StateHasChanged();
    }


    private Task OpenCreatePackageTypeDialog(DialogOptions options)
    {
        return Dialog.ShowAsync<AddPackageTypeDialog>("Add Package Type", options);
    }

    private async Task OpenEditPackageTypeDialog(PackageTypeDto packageTypeDto)
    {
        var parameters = new DialogParameters { ["Package Type"] = packageTypeDto };
        var dialog = await Dialog.ShowAsync<EditPackageTypeDialog>("Edit Package Type", parameters);
        var result = await dialog.Result;


    }

    private async Task OpenDetailsPackageTypeDialog(PackageTypeDto packageTypeDto)
    {
        var parameters = new DialogParameters { ["Package Type"] = packageTypeDto };
        var dialog = await Dialog.ShowAsync<DetailsPackageTypeDialog>("Package Type Details", parameters);
        var result = await dialog.Result;
    }
}

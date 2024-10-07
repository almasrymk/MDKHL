using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Blazor.Components.DataGrid;
using SmartParking.Localization;
using SmartParking.Packages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartParking.Blazor.Components.Pages.Packages;

public partial class Package
{

	private List<PackageDto> packages = new List<PackageDto>();
	private PackageDto View;
	private DialogOptions options = new DialogOptions { CloseOnEscapeKey = true };
    CreateUpdatePackageDto createUpdatePackageDto = new CreateUpdatePackageDto();
    private int TotalItem;
    public int CurrentPage { get; set; } = 1; // Default current page value is 10
    public int RowPerPage { get; set; } = 10; // Default value is 10
    private TableData itemData = new TableData();
    private string termSearch;
    private readonly IStringLocalizer<SmartParkingResource> _localizer;

    private async Task GetData()
	{
		var response = await packageAppService.GetListAsync(new PackageListDto());
		packages = response.Items.ToList();
		//StateHasChanged();
	}
	protected override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();
		await GetData();
	}

    private Task OnClickRowEdit(PackageDto row)
    {
        createUpdatePackageDto = new CreateUpdatePackageDto();
        createUpdatePackageDto.ArabicName = row.ArabicName;
        createUpdatePackageDto.EnglishName = row.EnglishName;
        createUpdatePackageDto.Price = row.Price;
        createUpdatePackageDto.Description = row.Description;
        createUpdatePackageDto.Status = row.Status;
        createUpdatePackageDto.PackageTypeId = row.PackageTypeId;
        createUpdatePackageDto.published = row.published;
        createUpdatePackageDto.PublishFrom = row.PublishFrom;
        createUpdatePackageDto.PublishTo = row.PublishTo;
        createUpdatePackageDto.OrgnizationId = row.OrgnizationId;
    var parameters = new DialogParameters
        {
            ["Package"] = createUpdatePackageDto,
            ["UpdatedId"] = row.Id
        };
        return DialogService.ShowAsync<EditPackageDialog>("Edit Package", parameters);
    }
    private Task OnClickRowView(PackageDto row)
    {
        var parameters = new DialogParameters
        {
            ["Package"] = row,
        };
        return DialogService.ShowAsync<DetailsPackageDialog>("View Package", parameters);
    }
    private Task CreatePackage() => DialogService.ShowAsync<AddPackageDialog>("Simple Dialog", options);
}



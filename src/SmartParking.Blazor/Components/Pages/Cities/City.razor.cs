using MudBlazor;
using SmartParking.Blazor.Components.Pages.Governorates;
using SmartParking.Lookups;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Blazor.Components.Pages.Cities;

public partial class City
{
	private IReadOnlyList<CityDto> cities = new List<CityDto>();
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

		cities = result.Items.ToList();
		StateHasChanged();
	}
	

	private Task OpenCreateCityDialog(DialogOptions options)
	{
		return Dialog.ShowAsync<AddCityDialog>("Add City", options);
	}

	private async Task OpenEditCityDialog(CityDto cityDto)
	{
		var parameters = new DialogParameters { ["City"] = cityDto };
		var dialog = await Dialog.ShowAsync<EditCityDialog>("Edit City", parameters);
		var result = await dialog.Result;


	}

	private async Task OpenDetailsCityDialog(CityDto cityDto)
	{
		var parameters = new DialogParameters { ["City"] = cityDto };
		var dialog = await Dialog.ShowAsync<DetailsCityDialog>("City Details", parameters);
		var result = await dialog.Result;


	}

}

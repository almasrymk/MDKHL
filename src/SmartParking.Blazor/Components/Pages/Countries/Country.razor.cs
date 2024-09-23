using MudBlazor;
using SmartParking.Blazor.Components.Pages.Governments;
using SmartParking.Lookups;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Blazor.Components.Pages.Countries;

public partial class Country
{
	private IReadOnlyList<CountryDto> countries = new List<CountryDto>();
	private int currentPage = 1;
	private int pageSize = 100;
	//Dialog

	private readonly DialogOptions _backdropClick = new() { BackdropClick = false };



	private async Task GetData()
	{
		var result = await AppService.GetListAsync(new PagedAndSortedResultRequestDto
		{
			MaxResultCount = pageSize,
			SkipCount = (currentPage - 1) * pageSize
		});

		countries = result.Items.ToList();
		StateHasChanged();
	}
	protected override async Task OnInitializedAsync()
	{

		await base.OnInitializedAsync();
		await GetData();

	}

	private Task OpenCreateCountryDialog(DialogOptions options)
	{
		return Dialog.ShowAsync<AddCountryDialog>("Add Country", options);
	}

	private async Task OpenEditCountryDialog(CountryDto countryDto)
	{
		var parameters = new DialogParameters { ["Country"] = countryDto };
		var dialog = await Dialog.ShowAsync<EditCountryDialog>("Edit Country", parameters);
		var result = await dialog.Result;
	}
	private async Task OpenDetailsCountryDialog(CountryDto countryDto)
	{
		var parameters = new DialogParameters { ["Country"] = countryDto };
		var dialog = await Dialog.ShowAsync<DetailsCountryDialog>("Details Country", parameters);
		var result = await dialog.Result;
	}
}

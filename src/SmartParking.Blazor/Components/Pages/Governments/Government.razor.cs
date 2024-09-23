using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Cameras;
using SmartParking.Lookups;
using SmartParking.Lookups.Governorate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;


namespace SmartParking.Blazor.Components.Pages.Governments;

public partial class Government
{

	private IReadOnlyList<GovernorateDto> governorates = new List<GovernorateDto>();
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

		governorates = result.Items.ToList(); 
		StateHasChanged();
	}
	protected override async Task OnInitializedAsync()
	{

		await base.OnInitializedAsync();
		await GetData();

	}

	private Task OpenCreateGovernmentDialog(DialogOptions options)
	{
		return Dialog.ShowAsync<EditGovernmentDialog>("Add Camera", options);
	}

	private async Task OpenEditGovernmentDialog(GovernorateDto governorateDto)
	{
		var parameters = new DialogParameters { ["Governorate"] = governorateDto };
		var dialog = await Dialog.ShowAsync<EditGovernmentDialog>("Edit Governorate", parameters);
		var result = await dialog.Result;


	}
	
	//public IReadOnlyList<GovernorateDto> items = Array.Empty<GovernorateDto>();
	//public IReadOnlyList<CountryLookupDto> countries = Array.Empty<CountryLookupDto>();
	//private readonly DialogOptions _backdropClick = new() { BackdropClick = false };


	//protected override async Task OnInitializedAsync()
	//{
	//    var input = new PagedAndSortedResultRequestDto();
	//    await base.OnInitializedAsync();
	//    items = (await AppService.GetListAsync(input)).Items;
	//    countries = (await AppService.GetCountryLookupAsync()).Items;
	//}


	//private async Task OpenCreateGovernmentDialog(DialogOptions dialogOptions)
	//{
	//     await Dialog.ShowAsync<AddGovernmentDialog>("Add",dialogOptions);
	//}




}


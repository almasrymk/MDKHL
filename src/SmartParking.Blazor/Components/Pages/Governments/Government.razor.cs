using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Pages.Cities;
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
		return Dialog.ShowAsync<AddGovernmentDialog>("Add Governorate", options);
	}

	private async Task OpenEditGovernmentDialog(GovernorateDto governorateDto)
	{
		var parameters = new DialogParameters { ["Governorate"] = governorateDto };
		var dialog = await Dialog.ShowAsync<EditGovernmentDialog>("Edit Governorate", parameters);
		var result = await dialog.Result;


	}

	private async Task OpenDetailsGovernmentDialog(GovernorateDto governorate)
	{
		var parameters = new DialogParameters { ["Governorate"] = governorate };
		var dialog = await Dialog.ShowAsync<DetailsGovernmentDialog>("Governorate Details", parameters);
		var result = await dialog.Result;


	}






}


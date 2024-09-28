using MudBlazor;
using SmartParking.Lookups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;


namespace SmartParking.Blazor.Components.Pages.Governorates;

public partial class Governorate
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

	private Task OpenCreateGovernorateDialog(DialogOptions options)
	{
		return Dialog.ShowAsync<AddGovernorateDialog>("Add Governorate", options);
	}

	private async Task OpenEditGovernorateDialog(GovernorateDto governorateDto)
	{
		var parameters = new DialogParameters { ["Governorate"] = governorateDto };
		var dialog = await Dialog.ShowAsync<EditGovernorateDialog>("Edit Governorate", parameters);
		var result = await dialog.Result;


	}

	private async Task OpenDetailsGovernorateDialog(GovernorateDto governorate)
	{
		var parameters = new DialogParameters { ["Governorate"] = governorate };
		var dialog = await Dialog.ShowAsync<DetailsGovernorateDialog>("Governorate Details", parameters);
		var result = await dialog.Result;


	}






}


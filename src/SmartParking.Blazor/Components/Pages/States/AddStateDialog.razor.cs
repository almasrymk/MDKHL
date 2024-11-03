using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.States;

public partial class AddStateDialog
{
    public CreateUpdateStateDto input = new CreateUpdateStateDto();

    public List<RegionLookupDto> regions = new List<RegionLookupDto>();

    private MudForm form;

    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; }
    bool _clearable;
    private string RegionSelectValue { get; set; }

    protected override async Task OnInitializedAsync()
    {
		regions = await StateAppService.GetRegionLookupAsync();
        await base.OnInitializedAsync();
    }

    private void SelectRegion(string region)
    {
        RegionSelectValue = region;
    }
    private async Task Submit()
    {
		var successedMessage = L["StateCreatedSuccessfully"];
		var errorMessage = L["Can'tCreateState"];
		var validationErrorMessage = L["PleaseFixTheValidationErrors"];


		// Call your service to create the state
		try
		{
			CreateUpdateStateDto createUpdateStateDto = new CreateUpdateStateDto()
			{
				ArabicName = input.ArabicName,
				EnglishName = input.EnglishName,
				Status = input.Status,
				RegionId = input.RegionId,
			};

			var states = await StateAppService.CreateAsync(createUpdateStateDto);
			if (states != null)
			{
				Snackbar.Add(successedMessage, Severity.Success);
				MudDialog.Close(DialogResult.Ok(true));
				await Task.Delay(1000);
				Navigation.NavigateTo("/states", true);
			}

			else
			{
				Snackbar.Add(errorMessage, Severity.Error);

			}
		}
		catch (AbpValidationException validationException)
		{
			foreach (var validationError in validationException.ValidationErrors)
			{
				Snackbar.Add($"{validationErrorMessage} {validationError.MemberNames.First()} - {validationError.ErrorMessage}", Severity.Error);
			}
		}
		catch (BusinessException ex)
		{
			Snackbar.Add($"{errorMessage} {ex.Code}", Severity.Error);

		}
		catch (Exception ex)
		{
			Snackbar.Add($"{errorMessage} {ex.Message}", Severity.Error);
		}

    }
    private void Cancel()
    {
        MudDialog.Cancel();
    }
}

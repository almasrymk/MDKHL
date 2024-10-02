using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Lookups.State;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.States;

public partial class AddStateDialog
{
    private MudForm form;
    public CreateUpdateStateDto input = new CreateUpdateStateDto();

    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; }
    bool _clearable;
    private int RegionSelectValue { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

    private void SelectRegion(int region)
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
				RegionId = RegionSelectValue
			};

			var regions = await StateAppService.CreateAsync(createUpdateStateDto);
			if (regions != null)
			{
				Snackbar.Add(successedMessage, Severity.Success);
				MudDialog.Close(DialogResult.Ok(true));
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
			Snackbar.Add($"Failed to add state: {ex.Code} + ", Severity.Error);

		}
		catch (Exception ex)
		{
			Snackbar.Add($"Failed to state: {ex.Message} + ", Severity.Error);
		}

    }
    private void Cancel()
    {
        MudDialog.Cancel();
    }
}

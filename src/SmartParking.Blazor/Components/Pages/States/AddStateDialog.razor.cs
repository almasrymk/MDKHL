using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Lookups.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
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
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.ADD]} {states.EnglishName}", Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
				await Task.Delay(1000);
				Navigation.NavigateTo("/states", true);
			}
		}
        catch (AbpValidationException validationException)
        {
            foreach (var validationError in validationException.ValidationErrors)
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.VAlIDATION_ERROR]}: {L[validationError.MemberNames.First()]} - {validationError.ErrorMessage}", Severity.Error);
            }
        }
         catch (BusinessException ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.ADD]} : {ex.Code}", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.ADD]} : {ex.Message} ", Severity.Error);
        }
        catch (Exception ex)
        {

            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.ADD]} : {ex.Message} ", Severity.Error);
        }
    }
    private void Cancel()
    {
        MudDialog.Cancel();
    }
}

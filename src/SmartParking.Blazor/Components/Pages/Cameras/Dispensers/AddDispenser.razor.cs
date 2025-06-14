using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Cameras;
using SmartParking.Cameras.Watchers.Dispensers;
using SmartParking.Garages;
using SmartParking.Lookups.CameraBrands;
using SmartParking.Lookups.CameraMainType;
using SmartParking.Lookups.CameraModel;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.Cameras.Dispensers;

public partial class AddDispenser
{
    private CreateUpdateDispenserDto DispenserDto = new CreateUpdateDispenserDto();
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }

    private MudForm form;
    private bool _clearable;
    public List<GarageDto> Garages { get; set; } = new List<GarageDto>();

   
    protected override async Task OnInitializedAsync()
    {
      

        var result = await GaragesAppService.GetListAsync(new PagedAndSortedResultRequestDto
        {
            MaxResultCount = 100,
            SkipCount = (1 - 1) * 100
        });
        Garages = result.Items.ToList();

        await base.OnInitializedAsync();
    }

    /// <summary>
    /// Submits the form data to create a new dispenser. Validates input and shows error or success messages accordingly.
    /// </summary>
    private async Task Submit()
    {
        try
        {
            var garage = Garages.Find(x => x.Id == DispenserDto.GarageId);
            if (garage == null)
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.ADD]} : {L["ChooseGarage"]}", Severity.Error);
            }
            else
            {
                DispenserDto.TenantId = garage.TenantId;
                DispenserDto.OrganizationId = garage.OrganizationId;

                var createdDispenserDto = await DispensersAppService.CreateAsync(DispenserDto);
                MudDialog.Close(DialogResult.Ok(true));
                SnackBar.ShowSnackbar(Snackbar, $"{createdDispenserDto.Name} {L[AlertMessages.AlertMessages.ADD]} {L[AlertMessages.AlertMessages.SUCCESS]}", Severity.Success);
                await Task.Delay(1000);
                Navigation.NavigateTo("/dispensers", forceLoad: true);
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
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]}{L[AlertMessages.AlertMessages.ADD]} : {ex.Code}", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]}{L[AlertMessages.AlertMessages.ADD]}: {ex.Message} ", Severity.Error);
        }
        catch (Exception ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.ADD]} : {ex.Message} ", Severity.Error);
        }
    }

    /// <summary>
    /// Cancels the operation and closes the dialog without saving.
    /// </summary>
    private void Cancel()
    {
        MudDialog.Cancel();
    }
}

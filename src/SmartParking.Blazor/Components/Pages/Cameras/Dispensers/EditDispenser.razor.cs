using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Cameras;
using SmartParking.Cameras.Watchers.Dispensers;
using SmartParking.Garages;
using SmartParking.Localization;
using SmartParking.Lookups.CameraBrands;
using SmartParking.Lookups.CameraMainType;
using SmartParking.Lookups.CameraModel;
using SmartParking.LookupsAppServices;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.Cameras.Dispensers;

public partial class EditDispenser
{
    [CascadingParameter]
    private IMudDialogInstance MudDialogAdd { get; set; }
    [Parameter]
    public CreateUpdateDispenserDto Dispenser { get; set; }
    [Parameter]
    public int UpdatedId { get; set; }
    public List<GarageDto> Garages { get; set; } = new List<GarageDto>();
    private bool _clearable;
    private MudForm form;
    private int OrgId { get; set; }
    private string GarageCode { get; set; }
    private bool isLoaded = false;
    private int GarageId { get; set; }
    /// <summary>
    /// Initializes data required for the form, such as camera brands, models, types, and garages.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {

        await GetAllGarage();
        StateHasChanged();
        await base.OnInitializedAsync();
    }

    private async Task GetAllGarage()
    {
        var result = await GaragesAppService.GetListAsync(new PagedAndSortedResultRequestDto
        {
            MaxResultCount = 100,
            SkipCount = 0
        });
        Garages = result.Items.ToList();
        var garage = Garages.Find(x => x.Id == Dispenser.GarageId);
        GarageCode = garage.Code;
        GarageId = garage.Id;
        Dispenser.OrganizationId = garage.OrganizationId;
    }

    
    /// <summary>
    /// Submits the edited dispenser information and handles validation and error messaging.
    /// </summary>
    private async Task SubmitEdit()
    {
        try
        {
            if (Dispenser.GarageId != GarageId)
            {
                var newGarage = Garages.Find(x => x.Id == Dispenser.GarageId);
                Dispenser.OrganizationId = newGarage.OrganizationId;
                Dispenser.TenantId = newGarage.TenantId;
            }
            if (string.IsNullOrEmpty(Dispenser.DeviceId))
            {
                SnackBar.ShowSnackbar(Snackbar,
                    $"{LocalizationHelper.GetLocalizedString("VaildtionError")}: " +
                    $"{LocalizationHelper.GetLocalizedString("DeviceId")} - " +
                    $"{LocalizationHelper.GetLocalizedString("RequiredValidation")}",
                    Severity.Error);
                return;
            }
            await DispensersAppService.UpdateAsync(UpdatedId, Dispenser);
            MudDialogAdd.Close(DialogResult.Ok(true));
            SnackBar.ShowSnackbar(Snackbar,
                $"{L[AlertMessages.AlertMessages.SUCCESS]} " +
                $"{L[AlertMessages.AlertMessages.UPDATE]} {Dispenser.Name}",
                Severity.Success);
            await Task.Delay(1000);
            Navigation.NavigateTo("/dispensers", forceLoad: true);
        }
        catch (AbpValidationException validationException)
        {
            foreach (var validationError in validationException.ValidationErrors)
            {
                SnackBar.ShowSnackbar(Snackbar,
                    $"{L[AlertMessages.AlertMessages.VAlIDATION_ERROR]}: " +
                    $"{L[validationError.MemberNames.First()]} - {validationError.ErrorMessage}",
                    Severity.Error);
            }
        }
        catch (BusinessException ex)
        {
            SnackBar.ShowSnackbar(Snackbar,
                $"{L[AlertMessages.AlertMessages.FAILED]} " +
                $"{L[AlertMessages.AlertMessages.UPDATE]} : {ex.Code}",
                Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            SnackBar.ShowSnackbar(Snackbar,
                $"{L[AlertMessages.AlertMessages.FAILED]} " +
                $"{L[AlertMessages.AlertMessages.UPDATE]} : {ex.Message}",
                Severity.Error);
        }
        catch (Exception ex)
        {
            SnackBar.ShowSnackbar(Snackbar,
                $"{L[AlertMessages.AlertMessages.FAILED]} " +
                $"{L[AlertMessages.AlertMessages.UPDATE]} : {ex.Message}",
                Severity.Error);
        }
    }

    /// <summary>
    /// Cancels the edit operation and closes the dialog.
    /// </summary>
    private async Task CancelEdit()
    {
        MudDialogAdd.Cancel();
    }

}

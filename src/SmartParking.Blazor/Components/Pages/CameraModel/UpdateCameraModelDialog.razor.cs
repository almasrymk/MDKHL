using FluentValidation;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Lookups.CameraBrands;
using SmartParking.Lookups.CameraModel;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.CameraModel;

/// <summary>
 /// Component for updating an existing camera model using a dialog form.
 /// </summary>
public partial class UpdateCameraModelDialog
{
    [Parameter]
    public CreateUpdateCameraModelDto cameraModel { get; set; } = new CreateUpdateCameraModelDto();

    [Parameter]
    public int Id { get; set; }

    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }

    private IList<CameraBrandDto> CameraBrands = new List<CameraBrandDto>();
    protected override async Task OnInitializedAsync()
    {
        CameraBrands = await CameraBrandAppService.GetAllCameraBrandsAsync();
        await base.OnInitializedAsync();
    }

    /// <summary>
    /// Submits the updated camera model data to the server and handles validation or business errors.
    /// </summary>
    private async Task Submit()
    {
        try
        {
            await CameraModelsAppService.UpdateAsync(Id, cameraModel);
            SnackBar.ShowSnackbar(Snackbar, $"{cameraModel.ArabicName} {L[AlertMessages.AlertMessages.UPDATE]} {L[AlertMessages.AlertMessages.SUCCESS]}", Severity.Success);
            await Task.Delay(1000);
            MudDialog.Close(DialogResult.Ok(true));
            Navigation.NavigateTo("/Cameramodels", true);
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
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.UPDATE]} : {ex.Code}", Severity.Error);
        }
        catch (AuthenticationException ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.UPDATE]} : {ex.Message} ", Severity.Error);
        }
        catch (Exception ex)
        {
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.UPDATE]} : {ex.Message} ", Severity.Error);
        }
    }

    /// <summary>
    /// Cancels the dialog without saving any changes.
    /// </summary>
    private void Cancel()
    {
        MudDialog.Cancel();
    }
}

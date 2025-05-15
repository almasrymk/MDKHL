using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Lookups.CameraBrands;
using SmartParking.Lookups.CameraModel;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.CameraModel
{
    /// <summary>
    /// Dialog component for adding a new camera model.
    /// </summary>
    partial class AddCameraModelDialog
    {
        private MudForm form;
        private CreateUpdateCameraModelDto cameraModel = new CreateUpdateCameraModelDto();
        private bool _clearable;
        private IList<CameraBrandDto> CameraBrands = new List<CameraBrandDto>();
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; }
        protected override async Task OnInitializedAsync()
        {
            CameraBrands = await CameraBrandAppService.GetAllCameraBrandsAsync();
            await base.OnInitializedAsync();
        }
        #region private method

        /// <summary>
        /// Handles form submission for creating a new camera model.
        /// Displays success or error notifications based on the outcome.
        /// </summary>
        /// <returns>A task that represents the asynchronous submit operation.</returns>
        private async Task Submit()
        {
            try
            {
                await CameraModelsAppService.CreateAsync(cameraModel);
                SnackBar.ShowSnackbar(Snackbar, $"{cameraModel.ArabicName} {L[AlertMessages.AlertMessages.ADD]} {L[AlertMessages.AlertMessages.SUCCESS]}", Severity.Success);
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

        /// <summary>
        /// Cancels the dialog without saving changes.
        /// </summary>
        private void Cancel()
        {
            MudDialog.Cancel();
        }

        #endregion
    }

}


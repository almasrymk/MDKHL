using System.Security.Authentication;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Lookups.CameraBrands;
using SmartParking.Lookups.CameraMainType;
using Volo.Abp;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.CameraMainType
{
    /// <summary>
    /// Dialog component for updating an existing camera main type.
    /// </summary>
    partial class UpdateCameraMainTypeDialog
    {
        [Parameter]
        public CreateUpdateCameraMainTypeDto cameraMainType { get; set; } = new CreateUpdateCameraMainTypeDto();
        [Parameter]
        public int Id { get; set; }
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; }

        private MudForm form;
        private bool _clearable;
        /// <summary>
        /// List of available camera brands to populate dropdowns or selection fields.
        /// </summary>
        IList<CameraBrandDto> CameraBrands = new List<CameraBrandDto>();

        /// <summary>
        /// Called when the component is initialized.
        /// Loads the available camera brands from the service.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            CameraBrands = await CameraBrandAppService.GetAllCameraBrandsAsync();
            await base.OnInitializedAsync();
        }

        /// <summary>
        /// Handles the submission of the updated camera main type.
        /// Sends update request to the server and shows appropriate snackbar notifications.
        /// Closes the dialog on success.
        /// </summary>
        private async Task Submit()
        {
            try
            {
                await CameraMainTypesAppService.UpdateAsync(Id, cameraMainType);
                SnackBar.ShowSnackbar(Snackbar, $"{cameraMainType.ArabicName} {L[AlertMessages.AlertMessages.UPDATE]} {L[AlertMessages.AlertMessages.SUCCESS]}", Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
                Navigation.NavigateTo("/Cameramaintypes", true);
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
        /// Cancels the edit camera main type dialog 
        /// </summary>
        private void Cancel()
        {
            MudDialog.Cancel();
        }
    }

}

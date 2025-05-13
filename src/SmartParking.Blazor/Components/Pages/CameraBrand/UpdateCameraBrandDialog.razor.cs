using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Lookups.CameraBrands;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.CameraBrand
{
    /// <summary>
    /// Partial class responsible for handling the update logic of a Camera Brand using a dialog UI.
    /// </summary>
    partial class UpdateCameraBrandDialog
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialogAdd { get; set; }
        [Parameter]
        public int Id { get; set; }
        [Parameter]
        public CreateUpdateCameraBrandDto CameraBrand { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }

        /// <summary>
        /// Handles form submission for updating the camera brand.
        /// Validates and submits the form, shows success or error messages accordingly.
        /// </summary>
        private async Task SubmitEdit()
        {
            try
            {
                await CameraBrandsAppService.UpdateAsync(Id, CameraBrand);
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.UPDATE]} {CameraBrand.ArabicName} ", Severity.Success);
                MudDialogAdd.Close(DialogResult.Ok(true));
                await Task.Delay(1000);
                Navigation.NavigateTo("/cameraBrands", true);
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
        /// Cancels the edit operation and closes the dialog.
        /// </summary>
        private async Task CancelEdit()
        {
            MudDialogAdd.Cancel();
        }
    }

}


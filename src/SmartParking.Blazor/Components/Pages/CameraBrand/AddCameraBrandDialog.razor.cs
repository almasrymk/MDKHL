using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Lookups.CameraBrands;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.CameraBrand
{
    partial class AddCameraBrandDialog
    {
        private MudForm form;
        private CreateUpdateCameraBrandDto cameraBrandDro = new CreateUpdateCameraBrandDto();
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; }
        bool _clearable;

        protected override async Task OnInitializedAsync()
        {

            await base.OnInitializedAsync();
        }
        private async Task Submit()
        {

            try
            {
                await CameraBrandsAppService.CreateAsync(cameraBrandDro);
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.ADD]} {cameraBrandDro.ArabicName} ", Severity.Success);

                MudDialog.Close(DialogResult.Ok(true));
                await Task.Delay(1000);
                Navigation.NavigateTo("/Camerabrands", true);
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
}

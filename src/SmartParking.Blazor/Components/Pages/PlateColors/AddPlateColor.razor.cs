using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Lookups.PlateColor;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.PlateColors
{
    partial class AddPlateColor
    {
        private MudForm form;
        private CreateUpdatePlateColorDto plateColorDro = new CreateUpdatePlateColorDto();
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; }
        bool _clearable;

        protected override async Task OnInitializedAsync()
        {

            await base.OnInitializedAsync();
        }
        private async Task Submit()
        {
            var successedMessage = L["PlateColorCreatedSuccessfully"];
            var errorMessage = L["FillRequriedItems"];
            var validationErrorMessage = L["PleaseFixTheValidationErrors"];
            if (form.IsValid)
            {
                // Call your service to create the camera
                try
                {
                    await PlateColorsAppService.CreateAsync(plateColorDro);
                    Snackbar.Add(successedMessage, Severity.Success);
                    MudDialog.Close(DialogResult.Ok(true));
                    await Task.Delay(1000);
                    Navigation.NavigateTo("/plateColors", true);
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
            else
            {
                Snackbar.Add(errorMessage, Severity.Error);
            }
        }
        private void Cancel()
        {
            MudDialog.Cancel();
        }
    }
}

using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Lookups.CameraBrands;
using SmartParking.Lookups.CameraMainType;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.CameraMainType
{
    partial class AddCameraMainTypeDialog
    {
        private MudForm form;
        private CreateUpdateCameraMainTypeDto cameraMainType = new CreateUpdateCameraMainTypeDto();
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; }
        bool _clearable;
        IList<CameraBrandDto> CameraBrands = new List<CameraBrandDto>();

        protected override async Task OnInitializedAsync()
        {
            CameraBrands = await CameraBrandAppService.GetAllCameraBrandsAsync();
            await base.OnInitializedAsync();
        }
        private async Task Submit()
        {


            try
            {
                await CameraMainTypesAppService.CreateAsync(cameraMainType);
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.ADD]} {cameraMainType.ArabicName} ", Severity.Success);
                await Task.Delay(1000);
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

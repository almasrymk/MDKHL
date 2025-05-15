using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Lookups.VehicleBrand;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.VehicleBrands
{
    partial class EditVehicleBrand
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialogAdd { get; set; }
        [Parameter] public CreateUpdateVehicleBrandDto VehicleBrand { get; set; }
        [Parameter]
        public int UpdatedId { get; set; }
        bool _clearable;
        private MudForm form;

        protected override async Task OnInitializedAsync()
        {

            await base.OnInitializedAsync();
        }

        private async Task SubmitEdit()
        {

            try
            {

                await VehicleBrandsAppService.UpdateAsync(UpdatedId, VehicleBrand);
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.UPDATE]} {VehicleBrand.ArabicName}", Severity.Success);
                MudDialogAdd.Close(DialogResult.Ok(true));
                await Task.Delay(1000);
                Navigation.NavigateTo("/vehicleBrands", true);

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

        private async Task CancelEdit()
        {
            MudDialogAdd.Cancel();


        }
    }
}

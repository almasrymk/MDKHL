using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Lookups.PlateColor;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.PlateColors
{
    partial class EditPlateColor
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialogAdd { get; set; }
        [Parameter] public CreateUpdatePlateColorDto PlateColor { get; set; }
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
            var successedMessage = L["PlateColorUpdatedSuccessfully"];

            try
            {

                await PlateColorsAppService.UpdateAsync(UpdatedId, PlateColor);
                Snackbar.Add(successedMessage, Severity.Success);
                MudDialogAdd.Close(DialogResult.Ok(true));
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

using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Shifts;
using SmartParking.Validations;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.Shifts
{
    partial class EditShiftDialog
    {
        [Parameter] public CreateUpdateShiftDto Shift { get; set; }
        [Parameter] public int UpdatedId { get; set; }
        [CascadingParameter]
        private IMudDialogInstance MudDialogAdd { get; set; }
        private TimeSpan? _timeSpanStart
        {
            get => Shift.StartTime.ToTimeSpan();
            set => Shift.StartTime = TimeOnly.FromTimeSpan(value.Value);
        }
        private TimeSpan? _timeSpanEnd
        {
            get => Shift.EndTime.ToTimeSpan();
            set => Shift.EndTime = TimeOnly.FromTimeSpan(value.Value);
        }
        private DateTime? _startDateTime
        {
            get => Shift.StartDate == default ? (DateTime?)null : Shift.StartDate.ToDateTime(new TimeOnly(0, 0)); // Convert DateOnly to DateTime
            set => Shift.StartDate = value.HasValue ? DateOnly.FromDateTime(value.Value) : default; // Convert DateTime to DateOnly
        }
        private DateTime? _endDateTime
        {
            get => Shift.EndDate == default ? (DateTime?)null : Shift.EndDate.ToDateTime(new TimeOnly(0, 0)); // Convert DateOnly to DateTime
            set => Shift.EndDate = value.HasValue ? DateOnly.FromDateTime(value.Value) : default; // Convert DateTime to DateOnly
        }
        private async Task SubmitEdit()
        {

            try
            {
                Shift.IsFullDay = Shift.IsFullDay || PackageValidations.CheckPackageTimeValiditity(Shift.StartTime, Shift.EndTime);
                if (CurrentTenant.Id != null)
                    await ShiftAppService.UpdateAsync(UpdatedId, Shift);
                else
                    await ShiftAppService.UpdateMDKHLAsync(UpdatedId, Shift);
                MudDialogAdd.Close(DialogResult.Ok(true));
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.UPDATE]} {Shift.ArabicName}", Severity.Success);
                await Task.Delay(1000);
                Navigation.NavigateTo("/shifts", forceLoad: true);


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

using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Garages;
using SmartParking.Shifts;
using SmartParking.Validations;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.Shifts
{
    partial class AddShiftDialog
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; }

        public List<GarageDto> Garages { get; set; } = new List<GarageDto>();

        private CreateUpdateShiftDto shiftDto = new CreateUpdateShiftDto();

        private TimeSpan? _timeSpanStart
        {
            get => shiftDto.StartTime.ToTimeSpan();
            set => shiftDto.StartTime = TimeOnly.FromTimeSpan(value.Value);
        }
        private TimeSpan? _timeSpanEnd
        {
            get => shiftDto.EndTime.ToTimeSpan();
            set => shiftDto.EndTime = TimeOnly.FromTimeSpan(value.Value);
        }
        private DateTime? _startDateTime
        {
            get => shiftDto.StartDate == default ? (DateTime?)null : shiftDto.StartDate.ToDateTime(new TimeOnly(0, 0)); // Convert DateOnly to DateTime
            set => shiftDto.StartDate = value.HasValue ? DateOnly.FromDateTime(value.Value) : default; // Convert DateTime to DateOnly
        }
        private DateTime? _endDateTime
        {
            get => shiftDto.EndDate == default ? (DateTime?)null : shiftDto.EndDate.ToDateTime(new TimeOnly(0, 0)); // Convert DateOnly to DateTime
            set => shiftDto.EndDate = value.HasValue ? DateOnly.FromDateTime(value.Value) : default; // Convert DateTime to DateOnly
        }

        protected override async Task OnInitializedAsync()
        {
            var result = await GaragesAppService.GetListAsync(new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 100,
                SkipCount = (1 - 1) * 100
            });
            Garages = result.Items.ToList();
            await base.OnInitializedAsync();
        }
        private async Task Submit()
        {

            try
            {
                var garage = Garages.Find(x => x.Id == shiftDto.GarageId);
                shiftDto.OrganizationId = garage.OrganizationId;
                shiftDto.TenantId = CurrentTenant.Id;
                shiftDto.Status = Enums.Status.Inactive;
                shiftDto.IsFullDay = shiftDto.IsFullDay || PackageValidations.CheckPackageTimeValiditity(shiftDto.StartTime, shiftDto.EndTime);
                await ShiftAppService.CreateAsync(shiftDto);
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.ADD]} {shiftDto.ArabicName}", Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
                await Task.Delay(1000);
                Navigation.NavigateTo("/shifts", true);
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

                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.ADD]} : {ex.Message}", Severity.Error);
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

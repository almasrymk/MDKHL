using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Garages;
using SmartParking.Organizations;
using SmartParking.Shifts;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.Shifts
{
    partial class AddShiftDialogForMDKHL
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; }

        public List<OrganizationDto> Organizations { get; set; } = new List<OrganizationDto>();
        public IReadOnlyList<GarageDto> Garages { get; set; } = new List<GarageDto>();

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
            var result = await OrganizationAppService.GetListAsync(new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 100,
                SkipCount = (1 - 1) * 100
            });
            Organizations = result.Items.ToList();
            await base.OnInitializedAsync();
        }

        private async void ChangeOrganization(int OrganizationId)
        {
            shiftDto.OrganizationId = OrganizationId;
            shiftDto.GarageId = 0;
            var result = await GaragesAppService.GetGarageByOrganization(new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 100,
                SkipCount = (1 - 1) * 100
            }, OrganizationId);
            Garages = result.Items;
        }
        private async Task Submit()
        {

            try
            {
                shiftDto.TenantId = Organizations.Find(x => x.Id == shiftDto.OrganizationId).TenantId;
                shiftDto.Status = Enums.Status.Inactive;
                await ShiftAppService.CreateForMDKHLAsync(shiftDto);
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

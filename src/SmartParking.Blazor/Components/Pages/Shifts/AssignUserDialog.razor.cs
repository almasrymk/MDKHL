using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Entities;
using SmartParking.KeeperShifts;
using Volo.Abp;

namespace SmartParking.Blazor.Components.Pages.Shifts
{
    partial class AssignUserDialog
    {
        [CascadingParameter] IMudDialogInstance MudDialog { get; set; }

        [Parameter] public int ShiftId { get; set; }
        MudForm form;
        List<UserWithRole> Users = new();
        CreateUpdateKeeperShiftDto keeperShiftCreateDto = new();
        DateOnly startDate;
        DateOnly endDate;

        DateTime? _startDateTime
        {
            get => startDate == default ? (DateTime?)null : startDate.ToDateTime(new TimeOnly(0, 0));
            set => startDate = value.HasValue ? DateOnly.FromDateTime(value.Value) : default;
        }
        DateTime? _endDateTime
        {
            get => endDate == default ? (DateTime?)null : endDate.ToDateTime(new TimeOnly(0, 0));
            set => endDate = value.HasValue ? DateOnly.FromDateTime(value.Value) : default;
        }
        DateTime? _startDateTimeCreated
        {
            get => keeperShiftCreateDto.StartDate == default ? (DateTime?)null : keeperShiftCreateDto.StartDate.ToDateTime(new TimeOnly(0, 0));
            set => keeperShiftCreateDto.StartDate = value.HasValue ? DateOnly.FromDateTime(value.Value) : default;
        }
        DateTime? _endDateTimeCreated
        {
            get => keeperShiftCreateDto.EndDate == default ? (DateTime?)null : keeperShiftCreateDto.EndDate.ToDateTime(new TimeOnly(0, 0));
            set => keeperShiftCreateDto.EndDate = value.HasValue ? DateOnly.FromDateTime(value.Value) : default;
        }
        public string UserId { get; set; }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                var selectedShift = await ShiftAppService.GetShiftAsync(ShiftId);
                if (selectedShift != null)
                {
                    startDate = selectedShift.StartDate;
                    endDate = selectedShift.EndDate;
                    keeperShiftCreateDto.ShiftId = selectedShift.Id;
                    keeperShiftCreateDto.TenantId = selectedShift.TenantId.Value;
                }
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        protected override async Task OnInitializedAsync()
        {


            var input = new PagedAndSortedAndSearchResultRequestDto
            {
            };

            var result = CurrentTenant.Id == null ? await KeeperShiftsAppService.GetListUserCanAssignShiftsMdkhlAsync(CurrentTenant.Id, input) : (await KeeperShiftsAppService.GetListUserCanAssignShiftsOrganizationAsync(input));
            Users = result.Items.ToList();

        }

        private async Task OnUserChanged(string userId)
        {

            keeperShiftCreateDto.KeeperId = Guid.Parse(userId);
        }

        private async Task SubmitCreate()
        {
            try
            {
                bool success = await KeeperShiftsAppService.CreateAsync(keeperShiftCreateDto);
                if (success)
                {
                    MudDialog.Close(DialogResult.Ok(true));
                    SnackBar.ShowSnackbar(Snackbar, $"{L["User assigned to shift successfully"]}", Severity.Success);
                }
            }
            catch (BusinessException ex)
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L["Failed to assign user to shift"]}: {ex.Code}", Severity.Error);
            }
            catch (Exception ex)
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L["Failed to assign user to shift"]}: {ex.Message}", Severity.Error);
            }
        }

        private void Cancel()
        {
            MudDialog.Cancel();
        }
    }
}

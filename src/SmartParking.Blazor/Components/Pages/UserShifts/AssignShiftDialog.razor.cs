using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Blazor.Components.Shared;
using SmartParking.KeeperShifts;
using SmartParking.Shifts;
using System.Globalization;
using Volo.Abp;

namespace SmartParking.Blazor.Components.Pages.UserShifts;

public partial class AssignShiftDialog
{
    [CascadingParameter] IMudDialogInstance MudDialog { get; set; }

    [Parameter] public string UserId { get; set; }

    MudForm form;
    List<ShiftDto> Shifts = new();
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

    protected override async Task OnInitializedAsync()
    {
        var user = await UsersAppService.GetAsync(Guid.Parse(UserId));
        var organizationId = await OrganizationsAppService.GetOrganizationIdByTenant(user.TenantId);
        var shiftsList = await ShiftAppService.GetListMdkhlAsync(new PagedAndSortedAndSearchResultRequestDto(), user.TenantId);
        Shifts = shiftsList.Items.ToList();
        keeperShiftCreateDto.OrganizationId = organizationId;
        keeperShiftCreateDto.KeeperId = Guid.Parse(UserId);
    }

    private async Task OnShiftChanged(string shiftId)
    {
        keeperShiftCreateDto.ShiftId = int.Parse(shiftId);

        var selectedShift = Shifts.Find(s => s.Id == int.Parse(shiftId));

        if (selectedShift is not null)
        {
            startDate = selectedShift.StartDate;
            endDate = selectedShift.EndDate;
            keeperShiftCreateDto.TenantId = selectedShift.TenantId.Value;

        }
    }

    private async Task SubmitCreate()
    {
        try
        {
            bool success = await KeeperShiftsAppService.CreateAsync(keeperShiftCreateDto);
            if (success)
            {
                MudDialog.Close(DialogResult.Ok(true));
                if (CultureInfo.CurrentCulture.Name == "ar")
                {
                    SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.ASSIGN]}{" "} {L[AlertMessages.AlertMessages.SUCCESS]}", Severity.Success);
                }
                else
                {
                    SnackBar.ShowSnackbar(Snackbar, $" {L[AlertMessages.AlertMessages.SUCCESS]}{" "}{L[AlertMessages.AlertMessages.ASSIGN]}", Severity.Success);

                }

            }
        }
        catch (BusinessException ex)
        {
            if (CultureInfo.CurrentCulture.Name == "ar")
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]}{" "}{L[AlertMessages.AlertMessages.ASSIGN]} : {ex.Code}", Severity.Error);

            }
            else
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.ASSIGN]}{" "} {L[AlertMessages.AlertMessages.FAILED]}: {ex.Code}", Severity.Error);

            }
        }
        catch (Exception ex)
        {
            if (CultureInfo.CurrentCulture.Name == "ar")
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.FAILED]}{" "}{L[AlertMessages.AlertMessages.ASSIGN]} : {ex.Message}", Severity.Error);

            }
            else
            {
                SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.ASSIGN]}{" "} {L[AlertMessages.AlertMessages.FAILED]}: {ex.Message}", Severity.Error);

            }
        }

    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }
}

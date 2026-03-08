using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Authentication;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Entities.Cameras.Watchers;
using SmartParking.Garages;
using SmartParking.Shifts;
using SmartParking.Shifts.Extensions;
using SmartParking.Shifts.ShiftZones;
using SmartParking.Validations;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Validation;

namespace SmartParking.Blazor.Components.Pages.Shifts.ShiftZones;

public partial class AddShiftZoneDialog
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; }

    private CreateUpdateShiftZoneDto CreateUpdateShiftZone { get; set; } = new();
    private List<ShiftDto> ShiftDtos { get; set; } = new();
    private List<GarageDto> Garages { get; set; } = new();
    private List<AssignStaffDataDto> Keepers { get; set; } = new();
    private List<AssignStaffDataDto> Managers { get; set; } = new();
    private List<Watcher> Devices { get; set; } = new();
    private List<AssignmentRowDto> UiAssignments { get; set; } = new();

    private bool _isLoaded = false;
    private bool _isSubmitting = false;
    private bool _assignmentPanelOpen = true;

    private void ToggleAssignmentPanel() => _assignmentPanelOpen = !_assignmentPanelOpen;

    private Guid _selectedManagerId = Guid.Empty;
    private Guid SelectedManagerId
    {
        get => _selectedManagerId;
        set
        {
            if (_selectedManagerId == value) return;
            _selectedManagerId = value;
            ManagerChanged(value);
            CreateUpdateShiftZone.Manager = (value == Guid.Empty)
                ? null
                : Keepers.FirstOrDefault(x => x.Id == value)?.ToManagerDto();
        }
    }


    protected override async Task OnInitializedAsync()
    {
        var shiftsTask = ShiftAppService.GetListAsync(new PagedAndSortedResultRequestDto { MaxResultCount = 100 });
        var garagesTask = GarageAppService.GetListAsync(new PagedAndSortedResultRequestDto { MaxResultCount = 100 });
        var keepersTask = UserAppService.GetKeeperDataAsync();
        var watchersTask = WatcherAppService.GetAllWatchersAsync();

        await Task.WhenAll(shiftsTask, garagesTask, keepersTask, watchersTask);

        ShiftDtos = (await shiftsTask).Items.ToList();
        Garages = (await garagesTask).Items.ToList();
        Keepers = await keepersTask;
        Managers = Keepers;
        Devices = (await watchersTask).ToList();

        UiAssignments = new List<AssignmentRowDto> { new AssignmentRowDto() };

        _isLoaded = true;
    }


    private void ManagerChanged(Guid newManagerId)
    {
        foreach (var row in UiAssignments.Where(r => r.StaffId == newManagerId))
            row.StaffId = null;
    }

    private void AddRow() => UiAssignments.Add(new AssignmentRowDto());
    private void RemoveRow(AssignmentRowDto row) => UiAssignments.Remove(row);

    private bool IsKeeperDisabled(Guid keeperId, AssignmentRowDto currentRow)
        => keeperId == SelectedManagerId
        || UiAssignments.Any(r => r != currentRow && r.StaffId == keeperId);

    private bool IsDeviceDisabled(int deviceId, AssignmentRowDto currentRow)
        => UiAssignments.Any(r => r != currentRow && r.DeviceId == deviceId);


    public async Task Submit()
    {
        if (_isSubmitting) return;
        _isSubmitting = true;

        try
        {
            var (success, created) = await CreatingShiftZone();
            if (!success || created == null)
                return;

            await AssignShiftZoneDetails(created);

            SnackBar.ShowSnackbar(Snackbar,
                $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.ADD]} {CreateUpdateShiftZone.ShiftArabicName}",
                Severity.Success);

            MudDialog.Close(DialogResult.Ok(true));
            await Task.Delay(1000);
        }
        catch (AbpValidationException ex)
        {
            foreach (var error in ex.ValidationErrors)
                SnackBar.ShowSnackbar(Snackbar,
                    $"{L[AlertMessages.AlertMessages.VAlIDATION_ERROR]}: {L[error.MemberNames.First()]} - {error.ErrorMessage}",
                    Severity.Error);
        }
        catch (BusinessException ex)
        {
            SnackBar.ShowSnackbar(Snackbar,
                $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.ADD]} : {ex.Message}",
                Severity.Error);
        }
        catch (Exception ex)
        {
            SnackBar.ShowSnackbar(Snackbar,
                $"{L[AlertMessages.AlertMessages.FAILED]} {L[AlertMessages.AlertMessages.ADD]} : {ex.Message}",
                Severity.Error);
        }
        finally
        {
            _isSubmitting = false;
        }
    }

    public void Cancel() => MudDialog.Cancel();


    #region Private Methods
    private async Task<(bool Success, ShiftZoneDto? Created)> CreatingShiftZone()
    {
        if (CreateUpdateShiftZone.ZoneId == 0)
        {
            Snackbar.Add(L["PleaseSelectZone"], Severity.Warning);
            _isSubmitting = false;
            return (false, null);
        }

        CreateUpdateShiftZone.Status = Enums.Status.Inactive;
        CreateUpdateShiftZone.TenantId = CurrentTenant.Id;
        CreateUpdateShiftZone.IsFullDay = CreateUpdateShiftZone.IsFullDay
            || PackageValidations.CheckPackageTimeValiditity(
                   CreateUpdateShiftZone.StartTime, CreateUpdateShiftZone.EndTime);

        CreateUpdateShiftZone.OrganizationAssignmentDto(Garages[0]);

        var created = await ShiftZoneAppService.CreateAsync(CreateUpdateShiftZone);
        if (created == null)
        {
            Snackbar.Add(L["CreateFailed"], Severity.Error);
            _isSubmitting = false;
            return (false, null);
        }

        return (true, created);
    }

    private async Task AssignShiftZoneDetails(ShiftZoneDto created)
    {
        var rows = UiAssignments
            .Where(r => (r.StaffId != null && r.StaffId != Guid.Empty && r.StaffId != SelectedManagerId)
                     || (r.DeviceId != null && r.DeviceId > 0))
            .Select(r => r.ToZoneAssignmentDto(Keepers, Devices))
            .ToList();

        if (rows.Any())
            await AssignShiftZoneAppService.AddShiftZoneAssignmentsAsync(created.Id, rows);
    }
    #endregion
}
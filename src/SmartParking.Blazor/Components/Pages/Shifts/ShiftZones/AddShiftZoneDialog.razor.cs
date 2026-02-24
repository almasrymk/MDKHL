using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Authentication;
using SmartParking.Entities.Cameras.Watchers;
using SmartParking.Garages;
using SmartParking.Shifts;
using SmartParking.Shifts.ShiftDevices;
using SmartParking.Shifts.ShiftsMangers;
using SmartParking.Shifts.ShiftStaffDetail;
using SmartParking.Shifts.ShiftZones;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Blazor.Components.Pages.Shifts.ShiftZones;

public partial class AddShiftZoneDialog
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; }

    private CreateUpdateShiftZoneDto CreateUpdateShiftZone { get; set; } = new();
    private List<ShiftDto> ShiftDtos { get; set; } = new();
    private List<GarageDto> Garages { get; set; } = new();

    private List<AssignStaffDataDto> Keepers = new();
    private List<AssignStaffDataDto> Managers { get; set; } = new();
    private List<AssignStaffDataDto> Staffs { get; set; } = new();
    private List<Watcher> Devices { get; set; } = new();

    private bool _isLoaded = false;
    private bool _isSubmitting = false;
    private List<AssignmentRow> UiAssignments { get; set; } = new();

    public class AssignmentRow
    {
        public Guid? StaffId { get; set; }
        public int? DeviceId { get; set; }
    }

    private bool _assignmentPanelOpen { get; set; } = true;
    private void ToggleAssignmentPanel() => _assignmentPanelOpen = !_assignmentPanelOpen;

    private Guid _selectedManagerId = Guid.Empty;
    private Guid SelectedManagerId
    {
        get => _selectedManagerId;
        set
        {
            if (_selectedManagerId != value)
            {
                _selectedManagerId = value;
                ManagerChanged(value);
                var k = Keepers.FirstOrDefault(x => x.Id == value);
                CreateUpdateShiftZone.Manager = (k == null || value == Guid.Empty)
                    ? null
                    : new CreateUpdateShiftMangerDto
                    {
                        ManagerUserName = k.UserName,
                        ManagerEnName = k.EnglishName,
                        ManagerPhoneNumber = k.PhoneNumber
                    };
            }
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

        UiAssignments = new List<AssignmentRow> { new AssignmentRow() };

        _isLoaded = true;
    }

    private void ManagerChanged(Guid newManagerId)
    {
        foreach (var r in UiAssignments.Where(x => x.StaffId == newManagerId))
        {
            r.StaffId = null;
        }
    }

    private void AddRow()
    {
        UiAssignments.Add(new AssignmentRow());
    }

    private void RemoveRow(AssignmentRow row)
    {
        if (UiAssignments.Contains(row))
        {
            UiAssignments.Remove(row);
        }
    }

    private bool IsKeeperDisabled(Guid keeperId, AssignmentRow currentRow)
    {
        if (keeperId == SelectedManagerId) return true;

        return UiAssignments.Any(r => r != currentRow && r.StaffId == keeperId);
    }

    private bool IsDeviceDisabled(int deviceId, AssignmentRow currentRow)
    {
        return UiAssignments.Any(r => r != currentRow && r.DeviceId == deviceId);
    }

    public async Task Submit()
    {
        if (_isSubmitting) return;
        _isSubmitting = true;

        try
        {
            if (CreateUpdateShiftZone.ZoneId == 0)
            {
                Snackbar.Add(L["PleaseSelectZone"], Severity.Warning);
                _isSubmitting = false;
                return;
            }

            var created = await ShiftZoneAppService.CreateAsync(CreateUpdateShiftZone);
            if (created == null)
            {
                Snackbar.Add(L["CreateFailed"], Severity.Error);
                _isSubmitting = false;
                return;
            }

            var createdId = created.Id;

            var deviceDtos = UiAssignments
                .Where(r => r.DeviceId != null && r.DeviceId > 0)
                .Select(r =>
                {
                    var dev = Devices.FirstOrDefault(d => d.Id == r.DeviceId);
                    return new CreateUpdateShiftDeviceDto
                    {
                        DeviceId = r.DeviceId ?? 0,
                        DeviceCode = dev?.DeviceId,
                        DeviceUserName = dev?.Name,
                        DevicePassword = "P@ssw0rd"
                    };
                })
                .ToList();

            var staffDtos = UiAssignments
                .Where(r => r.StaffId != null && r.StaffId != Guid.Empty && r.StaffId != SelectedManagerId)
                .Select(r =>
                {
                    var k = Keepers.FirstOrDefault(x => x.Id == r.StaffId);
                    return new CreateUpdateShiftStaffDetailsDto
                    {
                        StaffId = r.StaffId ?? Guid.Empty,
                        StaffUserName = k?.UserName,
                        StaffEnName = k?.EnglishName,
                        StaffPhoneNumber = k?.PhoneNumber
                    };
                })
                .ToList();

            if (deviceDtos.Any())
            {
                var deviceRes = await AssignShiftZoneAppService.AssignDeviceToShiftZone(createdId, deviceDtos);
                if (deviceRes == null)
                {
                    Snackbar.Add(L["AssignDevicesFailed"], Severity.Error);
                }
            }

            if (staffDtos.Any())
            {
                var staffRes = await AssignShiftZoneAppService.AssignStaffToShiftZone(createdId, staffDtos);
                if (staffRes == null)
                {
                    Snackbar.Add(L["AssignStaffFailed"], Severity.Error);
                }
            }

            Snackbar.Add(L["ShiftZoneCreated"], Severity.Success);
            MudDialog.Close(DialogResult.Ok(true));
            await Task.Delay(1000);
            NavigationManager.NavigateTo("/shift-zone", true);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"{L["CreateFailed"]}: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isSubmitting = false;
        }
    }

    public void Cancel() => MudDialog.Cancel();
}
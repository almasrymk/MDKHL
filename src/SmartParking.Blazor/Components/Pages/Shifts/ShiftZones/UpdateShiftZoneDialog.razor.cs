using Microsoft.AspNetCore.Components;
using MudBlazor;
using SmartParking.Authentication;
using SmartParking.Entities.Cameras.Watchers;
using SmartParking.Garages;
using SmartParking.Shifts;
using SmartParking.Shifts.ShiftDetail;
using SmartParking.Shifts.ShiftDevices;
using SmartParking.Shifts.ShiftsMangers;
using SmartParking.Shifts.ShiftStaffDetail;
using SmartParking.Shifts.ShiftZones;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Blazor.Components.Pages.Shifts.ShiftZones;

public partial class UpdateShiftZoneDialog
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; }
    [Parameter] public Guid Id { get; set; }

    private CreateUpdateShiftZoneDto CreateUpdateShiftZone { get; set; } = new();
    private ShiftZoneDto ShiftZone { get; set; } = new();
    private List<ShiftDto> ShiftDtos = new();
    private List<GarageDto> Garages = new();
    private List<AssignStaffDataDto> Keepers = new();
    private List<AssignStaffDataDto> Managers = new();
    private List<Watcher> Devices = new();

    private List<CreateUpdateShiftDeviceDto> ShiftDeviceDtos = new();
    private List<CreateUpdateShiftStaffDetailsDto> ShiftStaffDetailsDtos = new();
    private List<ShiftDetailsDto> ShiftDetails = new();
    private Guid _selectedManagerId = Guid.Empty;


    private bool _isLoaded = false;
    private bool _isSubmitting = false;

    private bool _assignmentPanelOpen = true;
    private void ToggleAssignmentPanel() => _assignmentPanelOpen = !_assignmentPanelOpen;

    public class AssignmentRow
    {
        public Guid? StaffId { get; set; }
        public Guid? StaffEntityId { get; set; }
        public int? DeviceId { get; set; }
        public Guid? DeviceEntityId { get; set; }
    }

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
    private List<AssignmentRow> UiAssignments { get; set; } = new();
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

        ShiftZone = await ShiftZoneAppService.GetAsync(Id);
        ShiftDetails = ShiftZone.ShiftDetailsDto ?? new List<ShiftDetailsDto>();

        CreateUpdateShiftZone.ShiftId = ShiftZone.ShiftDto?.Id ?? 0;
        CreateUpdateShiftZone.ZoneId = ShiftZone.ZoneId;
        CreateUpdateShiftZone.ZoneArName = ShiftZone.ZoneArName;
        CreateUpdateShiftZone.ZoneEnName = ShiftZone.ZoneEnName;

        var managerDto = ShiftZone.ShiftMangerDto;
        CreateUpdateShiftZone.Manager = new CreateUpdateShiftMangerDto
        {
            ManagerUserName = managerDto?.ManagerUserName,
            ManagerEnName = managerDto?.ManagerEnName,
            ManagerPhoneNumber = managerDto?.ManagerPhoneNumber
        };

        if (managerDto != null)
        {
            if (!Keepers.Any(k => k.UserName == managerDto.ManagerUserName))
            {
                Keepers.Add(new AssignStaffDataDto
                {
                    Id = Guid.NewGuid(), 
                    UserName = managerDto.ManagerUserName,
                    EnglishName = managerDto.ManagerEnName,
                    PhoneNumber = managerDto.ManagerPhoneNumber
                });
            }
        }

        _selectedManagerId =
    Keepers.FirstOrDefault(x => x.UserName == managerDto?.ManagerUserName)?.Id
    ?? Guid.Empty;

        //var staffList = ShiftZone.ShiftDetailsDto ?? new List<ShiftStaffDetailsDto>();
        //var deviceList = ShiftDetails.ShiftDevices ?? new List<ShiftDeviceDto>();

        UiAssignments = new();

        for (int i = 0; i < ShiftZone.ShiftDetailsDto.Count; i++)
        {
            UiAssignments.Add(new AssignmentRow
            {
                StaffId = i < ShiftZone.ShiftDetailsDto.Count ? ShiftZone.ShiftDetailsDto[i].ShiftStaffDetails.StaffId : null,
                StaffEntityId = i < ShiftZone.ShiftDetailsDto.Count ? ShiftZone.ShiftDetailsDto[i].ShiftStaffDetails.Id : null, 
                DeviceId = i < ShiftZone.ShiftDetailsDto.Count ? ShiftZone.ShiftDetailsDto[i].ShiftDevices.DeviceId : null,
                DeviceEntityId = i < ShiftZone.ShiftDetailsDto.Count ? ShiftZone.ShiftDetailsDto[i].ShiftDevices.Id : null 
            });
        }


        if (!UiAssignments.Any())
            UiAssignments.Add(new AssignmentRow());

       

        _isLoaded = true;
        StateHasChanged();
    }

    private void ManagerChanged(Guid newManagerId)
    {
        foreach (var r in UiAssignments.Where(r => r.StaffId == newManagerId))
            r.StaffId = null;
    }

    private void AddRow() => UiAssignments.Add(new AssignmentRow());
    private async Task RemoveRow(AssignmentRow row)
    {
        if (UiAssignments.Contains(row))
            UiAssignments.Remove(row);
    }


    private bool IsKeeperDisabled(Guid keeperId, AssignmentRow row)
    {
        if (keeperId == SelectedManagerId) return true;
        return UiAssignments.Any(r => r != row && r.StaffId == keeperId);
    }

    private bool IsDeviceDisabled(int deviceId, AssignmentRow row)
        => UiAssignments.Any(r => r != row && r.DeviceId == deviceId);

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

            var mgr = Keepers.FirstOrDefault(x => x.Id == SelectedManagerId);
            CreateUpdateShiftZone.Manager = new CreateUpdateShiftMangerDto
            {
                ManagerUserName = mgr?.UserName,
                ManagerEnName = mgr?.EnglishName,
                ManagerPhoneNumber = mgr?.PhoneNumber
            };

            await ShiftZoneAppService.UpdateAsync(Id, CreateUpdateShiftZone);


            var deviceDtos = UiAssignments
                .Where(r => r.DeviceId != null || r.DeviceEntityId != null) 
                .Select(r => new CreateUpdateShiftDeviceDto
                {
                    Id = r.DeviceEntityId,
                    DeviceId = r.DeviceId ?? 0,
                    DeviceCode = Devices.FirstOrDefault(d => d.Id == r.DeviceId)?.DeviceId,
                    DeviceUserName = Devices.FirstOrDefault(d => d.Id == r.DeviceId)?.Name,
                    DevicePassword = "P@ssw0rd",
                }).ToList();

            var staffDtos = UiAssignments
                .Where(r => r.StaffId != null || r.StaffEntityId != null)
                .Select(r => new CreateUpdateShiftStaffDetailsDto
                {
                    Id = r.StaffEntityId,
                    StaffId = r.StaffId ?? Guid.Empty,
                    StaffUserName = r.StaffId != null ? Keepers.First(k => k.Id == r.StaffId).UserName : "",
                    StaffEnName = r.StaffId != null ? Keepers.First(k => k.Id == r.StaffId).EnglishName : "",
                    StaffPhoneNumber = r.StaffId != null ? Keepers.First(k => k.Id == r.StaffId).PhoneNumber : "",
                }).ToList();


            await AssignShiftZoneAppService.SyncDevicesForShiftZone(Id, deviceDtos);
            await AssignShiftZoneAppService.SyncStaffForShiftZone(Id, staffDtos);

            Snackbar.Add(L["ShiftZoneSaved"], Severity.Success);
            MudDialog.Close(DialogResult.Ok(true));
            await Task.Delay(1000);
            NavigationManager.NavigateTo("/shift-zone", true);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"{L["SaveFailed"]}: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isSubmitting = false;
        }
    }

    public void Cancel() => MudDialog.Cancel();
}
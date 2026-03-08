using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MudBlazor;
using SmartParking.Authentication;
using SmartParking.Blazor.Components.Shared;
using SmartParking.Entities.Cameras.Watchers;
using SmartParking.Entities.Shifts;
using SmartParking.Garages;
using SmartParking.Shifts;
using SmartParking.Shifts.ShiftDetail;
using SmartParking.Shifts.ShiftDevices;
using SmartParking.Shifts.ShiftsMangers;
using SmartParking.Shifts.ShiftStaffDetail;
using SmartParking.Shifts.ShiftZones;
using SmartParking.Validations;
using System.Security.Authentication;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Validation;

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
        public Guid? ExistingShiftDetailsId { get; set; } 
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
                        ManagerUserId = k.Id,
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

        CreateUpdateShiftZone.ShiftId = ShiftZone.ShiftId;
        CreateUpdateShiftZone.ZoneId = ShiftZone.ZoneId;
        CreateUpdateShiftZone.ZoneArName = ShiftZone.ZoneArName;
        CreateUpdateShiftZone.ZoneEnName = ShiftZone.ZoneEnName;

        var managerDto = ShiftZone.ShiftMangerDto;
        CreateUpdateShiftZone.Manager = new CreateUpdateShiftMangerDto
        {
            ManagerUserId = managerDto?.Id,
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
                    Id = managerDto.Id,
                    UserName = managerDto.ManagerUserName,
                    EnglishName = managerDto.ManagerEnName,
                    PhoneNumber = managerDto.ManagerPhoneNumber
                });
            }
        }

        _selectedManagerId =
    Keepers.FirstOrDefault(x => x.UserName == managerDto?.ManagerUserName)?.Id
    ?? Guid.Empty;


        UiAssignments = new();
        foreach (var detail in ShiftZone.ShiftDetailsDto ?? new List<ShiftDetailsDto>())
        {
            UiAssignments.Add(new AssignmentRow
            {
                ExistingShiftDetailsId = detail.Id,                       
                StaffId = detail.ShiftStaffDetails?.StaffId,
                StaffEntityId = detail.ShiftStaffDetails?.Id,
                DeviceId = detail.ShiftDevices?.DeviceId,
                DeviceEntityId = detail.ShiftDevices?.Id
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
            CreateUpdateShiftZone.TenantId = CurrentTenant.Id;
            CreateUpdateShiftZone.IsFullDay = CreateUpdateShiftZone.IsFullDay || PackageValidations.CheckPackageTimeValiditity(CreateUpdateShiftZone.StartTime, CreateUpdateShiftZone.EndTime);

            CreateUpdateShiftZone.OrganizationId = Garages[0].Id;
            CreateUpdateShiftZone.OrganizationEnName = Garages[0].OrganizationNameEn;
            CreateUpdateShiftZone.OrganizationArName = Garages[0].OrganizationNameAr;
            if (CreateUpdateShiftZone.ZoneId == 0)
            {
                Snackbar.Add(L["PleaseSelectZone"], Severity.Warning);
                _isSubmitting = false;
                return;
            }

            var mgr = Keepers.FirstOrDefault(x => x.Id == SelectedManagerId);
            CreateUpdateShiftZone.Manager = new CreateUpdateShiftMangerDto
            {
                ManagerUserId = mgr?.Id,
                ManagerUserName = mgr?.UserName,
                ManagerEnName = mgr?.EnglishName,
                ManagerPhoneNumber = mgr?.PhoneNumber
            };

            await ShiftZoneAppService.UpdateAsync(Id, CreateUpdateShiftZone);

            var assignmentRows = UiAssignments
                .Where(r => (r.StaffId != null && r.StaffId != Guid.Empty)
                         || (r.DeviceId != null && r.DeviceId > 0)
                         || r.ExistingShiftDetailsId != null) 
                .Select(r =>
                {
                    var keeper = r.StaffId != null ? Keepers.FirstOrDefault(k => k.Id == r.StaffId) : null;
                    var device = r.DeviceId != null ? Devices.FirstOrDefault(d => d.Id == r.DeviceId) : null;
                    return new CreateUpdateZoneAssignmentDto
                    {
                        ExistingShiftDetailsId = r.ExistingShiftDetailsId,
                        StaffId = r.StaffId,
                        StaffUserName = keeper?.UserName,
                        StaffEnName = keeper?.EnglishName,
                        StaffPhoneNumber = keeper?.PhoneNumber,
                        DeviceId = r.DeviceId,
                        DeviceCode = device?.DeviceId,
                        DeviceUserName = device?.Name,
                        DevicePassword = "P@ssw0rd"
                    };
                })
                .ToList();

            if (assignmentRows.Any())
                await AssignShiftZoneAppService.UpdateShiftZoneAssignmentsAsync(Id, assignmentRows);

            MudDialog.Close(DialogResult.Ok(true));
            SnackBar.ShowSnackbar(Snackbar, $"{L[AlertMessages.AlertMessages.SUCCESS]} {L[AlertMessages.AlertMessages.UPDATE]} {CreateUpdateShiftZone.ShiftArabicName}", Severity.Success);
            await Task.Delay(1000);
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
        finally
        {
            _isSubmitting = false;
        }
    }
    public void Cancel() => MudDialog.Cancel();
}
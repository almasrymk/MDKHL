using SmartParking.Results;
using SmartParking.Shifts.ShiftDevices;
using SmartParking.Shifts.ShiftStaffDetail;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartParking.Shifts.ShiftZones;

public interface IAssignShiftZoneAppService
{
    /// <summary>
    /// Method to assign a staff members to shift zone
    /// </summary>
    /// <param name="ShiftzoneId"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    Task<Result<bool>> AssignStaffToShiftZone(Guid ShiftzoneId, ICollection<CreateUpdateShiftStaffDetailsDto> dto);
    Task<Result<bool>> AssignDeviceToShiftZone(Guid ShiftzoneId, ICollection<CreateUpdateShiftDeviceDto> dtos);

    Task<Result<bool>> SyncStaffForShiftZone(Guid shiftZoneId, ICollection<CreateUpdateShiftStaffDetailsDto> incomingDtos);
    Task<Result<bool>> SyncDevicesForShiftZone(Guid shiftZoneId, ICollection<CreateUpdateShiftDeviceDto> incomingDtos);

}
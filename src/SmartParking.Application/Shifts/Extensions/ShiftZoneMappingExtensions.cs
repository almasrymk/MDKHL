using SmartParking.Entities.Shifts;
using System;
using Volo.Abp.Guids;

namespace SmartParking.Shifts.Extensions;

public static class ShiftZoneMappingExtensions
{
    public static ShiftDetails ToShiftDetails(this CreateUpdateZoneAssignmentDto dto, Guid shiftZoneId, Guid? tenantId, IGuidGenerator guidGenerator)
    {
        return new ShiftDetails(guidGenerator.Create()) 
        {
            ShiftZoneId = shiftZoneId,
            TenantId = tenantId,
            ShiftDevices = dto.DeviceId.HasValue && dto.DeviceId > 0
                ? dto.ToShiftDevice(tenantId.Value, guidGenerator)
                : null,
            ShiftStaffDetails = dto.StaffId.HasValue && dto.StaffId != Guid.Empty
                ? dto.ToShiftStaffDetails(tenantId.Value, guidGenerator)
                : null
        };
    }

    public static ShiftDevice ToShiftDevice(this CreateUpdateZoneAssignmentDto dto, Guid? tenantId, IGuidGenerator guidGenerator)
    {
        return new ShiftDevice(guidGenerator.Create()) 
        {
            ShiftDetailsId = dto.ExistingShiftDetailsId ?? Guid.Empty,
            DeviceId = dto.DeviceId!.Value,
            DeviceCode = dto.DeviceCode,
            DeviceUserName = dto.DeviceUserName,
            DevicePassword = dto.DevicePassword ?? "P@ssw0rd",
            TenantId = tenantId
        };
    }

    public static ShiftStaffDetails ToShiftStaffDetails(this CreateUpdateZoneAssignmentDto dto, Guid tenantId, IGuidGenerator guidGenerator)
    {
        return new ShiftStaffDetails(guidGenerator.Create()) 
        {
            ShiftDetailsId = dto.ExistingShiftDetailsId ?? Guid.Empty,
            StaffId = dto.StaffId!.Value,
            StaffEnName = dto.StaffEnName,
            StaffArName = dto.StaffUserName,
            StaffPhoneNumber = dto.StaffPhoneNumber,
            TenantId = tenantId
        };
    }
}
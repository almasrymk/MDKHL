using SmartParking.Authentication;
using SmartParking.Entities.Cameras.Watchers;
using SmartParking.Entities.Shifts;
using SmartParking.Garages;
using SmartParking.Shifts.ShiftDetail;
using SmartParking.Shifts.ShiftsMangers;
using SmartParking.Shifts.ShiftZones;
using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Guids;

namespace SmartParking.Shifts.Extensions;

public static class ShiftZoneMappingExtensions
{
    /// <summary>
    /// Maps a keeper record to a manager DTO.
    /// Returns null when <paramref name="staff"/> is null.
    /// </summary>
    public static CreateUpdateShiftMangerDto? ToManagerDto(this AssignStaffDataDto? staff)
    {
        if (staff is null) return null;

        return new CreateUpdateShiftMangerDto
        {
            ManagerUserId = staff.Id,
            ManagerUserName = staff.UserName,
            ManagerEnName = staff.EnglishName,
            ManagerPhoneNumber = staff.PhoneNumber
        };
    }
    /// <summary>
    /// Maps an existing manager read-DTO to its create/update counterpart.
    /// Returns an empty (all-null) DTO when <paramref name="managerDto"/> is null.
    /// </summary>
    public static CreateUpdateShiftMangerDto ToCreateUpdateDto(this ShiftMangerDto? managerDto)
    {
        if (managerDto is null) return new CreateUpdateShiftMangerDto();

        return new CreateUpdateShiftMangerDto
        {
            ManagerUserId = managerDto.Id,
            ManagerUserName = managerDto.ManagerUserName,
            ManagerEnName = managerDto.ManagerEnName,
            ManagerPhoneNumber = managerDto.ManagerPhoneNumber
        };
    }
    /// <summary>
    /// Converts a manager DTO to the lightweight staff-data shape used by the
    /// keeper drop-downs, so the current manager always appears as a selectable
    /// option even when absent from the fetched keeper list.
    /// </summary>
    public static AssignStaffDataDto ToAssignStaffDataDto(this ShiftMangerDto managerDto)
        => new AssignStaffDataDto
        {
            Id = managerDto.Id,
            UserName = managerDto.ManagerUserName,
            EnglishName = managerDto.ManagerEnName,
            PhoneNumber = managerDto.ManagerPhoneNumber
        };
    /// <summary>
    /// Produces a <see cref="CreateUpdateShiftZoneDto"/> pre-populated with the
    /// current persisted values of a shift-zone, ready for the edit form.
    /// Manager is fully mapped here — do not re-map it manually afterwards.
    /// </summary>
    public static CreateUpdateShiftZoneDto ToCreateUpdateDto(this ShiftZoneDto shiftZone)
        => new CreateUpdateShiftZoneDto
        {
            ShiftId = shiftZone.ShiftId,
            ZoneId = shiftZone.ZoneId,
            ZoneArName = shiftZone.ZoneArName,
            ZoneEnName = shiftZone.ZoneEnName,
            Manager = shiftZone.ShiftMangerDto.ToCreateUpdateDto()
        };
    /// <summary>
    /// Enriches a <see cref="CreateUpdateShiftZoneDto"/> in-place with
    /// denormalised shift and zone data. Used in the app-service Create/Update.
    /// </summary>
    public static void EnrichFrom(
        this CreateUpdateShiftZoneDto dto,
        ShiftDto shift,
        GarageDto garage)
    {
        dto.ZoneArName = garage.NameAr;
        dto.ZoneEnName = garage.NameEn;
        dto.StartDate = shift.StartDate;
        dto.EndDate = shift.EndDate;
        dto.StartTime = shift.StartTime;
        dto.EndTime = shift.EndTime;
        dto.IsFullDay = shift.IsFullDay;
        dto.ShiftEnglishName = shift.EnglishName;
        dto.ShiftArabicName = shift.ArabicName;
    }
    /// <summary>
    /// Writes the three organisation fields from the default garage into the DTO.
    /// Call this before sending the DTO to the app-service.
    /// </summary>
    public static void EnrichFromOrganization(
        this CreateUpdateShiftZoneDto dto,
        GarageDto organization)
    {
        dto.OrganizationId = organization.Id;
        dto.OrganizationEnName = organization.OrganizationNameEn;
        dto.OrganizationArName = organization.OrganizationNameAr;
    }
    /// <summary>
    /// Projects persisted shift-detail DTOs to the flat row model used by the
    /// assignment grid, preserving all entity IDs needed for subsequent updates.
    /// </summary>
    public static List<AssignmentRowDto> ToAssignmentRows(
        this IEnumerable<ShiftDetailsDto> details)
        => details.Select(d => new AssignmentRowDto
        {
            ExistingShiftDetailsId = d.Id,
            StaffId = d.ShiftStaffDetails?.StaffId,
            StaffEntityId = d.ShiftStaffDetails?.Id,
            DeviceId = d.ShiftDevices?.DeviceId,
            DeviceEntityId = d.ShiftDevices?.Id
        }).ToList();

    /// <summary>
    /// Builds a <see cref="CreateUpdateZoneAssignmentDto"/> from an assignment
    /// row, resolving staff and device details from the provided lookups.
    /// </summary>
    public static CreateUpdateZoneAssignmentDto ToZoneAssignmentDto(
        this AssignmentRowDto row,
        IReadOnlyList<AssignStaffDataDto> keepers,
        IReadOnlyList<Watcher> devices)
    {
        var keeper = row.StaffId is not null
            ? keepers.FirstOrDefault(k => k.Id == row.StaffId)
            : null;

        var device = row.DeviceId is not null
            ? devices.FirstOrDefault(d => d.Id == row.DeviceId)
            : null;

        return new CreateUpdateZoneAssignmentDto
        {
            ExistingShiftDetailsId = row.ExistingShiftDetailsId,
            StaffId = row.StaffId,
            StaffUserName = keeper?.UserName,
            StaffEnName = keeper?.EnglishName,
            StaffPhoneNumber = keeper?.PhoneNumber,
            DeviceId = row.DeviceId,
            DeviceCode = device?.DeviceId,
            DeviceUserName = device?.Name,
            DevicePassword = "P@ssw0rd"
        };
    }
    /// <summary>
    /// Maps a create/update DTO to a new <see cref="ShiftDetails"/> entity
    /// ready for persistence, including its optional staff and device children.
    /// </summary>
    public static ShiftDetails ToShiftDetails(
        this CreateUpdateZoneAssignmentDto dto,
        Guid shiftZoneId,
        Guid? tenantId,
        IGuidGenerator guidGenerator)
        => new ShiftDetails(guidGenerator.Create())
        {
            ShiftZoneId = shiftZoneId,
            TenantId = tenantId,
            ShiftDevices = dto.DeviceId.HasValue && dto.DeviceId > 0
                                    ? dto.ToShiftDevice(tenantId, guidGenerator)
                                    : null,
            ShiftStaffDetails = dto.StaffId.HasValue && dto.StaffId != Guid.Empty
                                    ? dto.ToShiftStaffDetails(tenantId, guidGenerator)
                                    : null
        };

    /// <summary>Maps a create/update DTO to a new <see cref="ShiftDevice"/> entity.</summary>
    public static ShiftDevice ToShiftDevice(
        this CreateUpdateZoneAssignmentDto dto,
        Guid? tenantId,
        IGuidGenerator guidGenerator)
        => new ShiftDevice(guidGenerator.Create())
        {
            ShiftDetailsId = dto.ExistingShiftDetailsId ?? Guid.Empty,
            DeviceId = dto.DeviceId!.Value,
            DeviceCode = dto.DeviceCode,
            DeviceUserName = dto.DeviceUserName,
            DevicePassword = dto.DevicePassword ?? "P@ssw0rd",
            TenantId = tenantId
        };

    /// <summary>Maps a create/update DTO to a new <see cref="ShiftStaffDetails"/> entity.</summary>
    public static ShiftStaffDetails ToShiftStaffDetails(
        this CreateUpdateZoneAssignmentDto dto,
        Guid? tenantId,
        IGuidGenerator guidGenerator)
        => new ShiftStaffDetails(guidGenerator.Create())
        {
            ShiftDetailsId = dto.ExistingShiftDetailsId ?? Guid.Empty,
            StaffId = dto.StaffId!.Value,
            StaffEnName = dto.StaffEnName,
            StaffArName = dto.StaffUserName,
            StaffPhoneNumber = dto.StaffPhoneNumber,
            TenantId = tenantId
        };
}
using SmartParking.Entities.Shifts;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Shifts.ShiftZones;

public class AssignShiftZoneAppService : ApplicationService, IAssignShiftZoneAppService
{
    private readonly IRepository<ShiftZone, Guid> _shiftZoneRepository;
    private readonly IRepository<ShiftDetails, Guid> _shiftDetailsRepository;
    private readonly IRepository<ShiftDevice, Guid> _shiftDeviceRepository;
    private readonly IRepository<ShiftStaffDetails, Guid> _shiftStaffDetailsRepository;

    public AssignShiftZoneAppService(
        IRepository<ShiftZone, Guid> shiftZoneRepository,
        IRepository<ShiftDetails, Guid> shiftDetailsRepository,
        IRepository<ShiftDevice, Guid> shiftDeviceRepository,
        IRepository<ShiftStaffDetails, Guid> shiftStaffDetailsRepository)
    {
        _shiftZoneRepository = shiftZoneRepository;
        _shiftDetailsRepository = shiftDetailsRepository;
        _shiftDeviceRepository = shiftDeviceRepository;
        _shiftStaffDetailsRepository = shiftStaffDetailsRepository;
    }

    //[UnitOfWork]
    public async Task<Result<bool>> AddShiftZoneAssignmentsAsync(
    Guid shiftZoneId,
    ICollection<CreateUpdateZoneAssignmentDto> rows)
    {
        var shiftZone = await _shiftZoneRepository.FindAsync(shiftZoneId);
        if (shiftZone is null)
            return Result.Error("Shift zone not found.");

        var detailsList = rows.Select(row => new ShiftDetails
        {
            ShiftZoneId = shiftZoneId,
            TenantId = shiftZone.TenantId,
        }).ToList();

        await _shiftDetailsRepository.InsertManyAsync(detailsList, autoSave: true);

        var devices = rows
            .Zip(detailsList, (row, detail) => (row, detail))
            .Where(x => x.row.DeviceId is > 0)
            .Select(x => new ShiftDevice
            {
                ShiftDetailsId = x.detail.Id,
                DeviceId = x.row.DeviceId!.Value,
                DeviceCode = x.row.DeviceCode,
                DeviceUserName = x.row.DeviceUserName,
                DevicePassword = x.row.DevicePassword ?? "P@ssw0rd",
                TenantId = shiftZone.TenantId,
            }).ToList();

        var staff = rows
            .Zip(detailsList, (row, detail) => (row, detail))
            .Where(x => x.row.StaffId is not null && x.row.StaffId != Guid.Empty)
            .Select(x => new ShiftStaffDetails
            {
                ShiftDetailsId = x.detail.Id,
                StaffId = x.row.StaffId!.Value,
                StaffEnName = x.row.StaffEnName,
                StaffArName = x.row.StaffUserName,
                StaffPhoneNumber = x.row.StaffPhoneNumber,
                TenantId = shiftZone.TenantId,
            }).ToList();

        if (devices.Any())
            await _shiftDeviceRepository.InsertManyAsync(devices);

        if (staff.Any())
            await _shiftStaffDetailsRepository.InsertManyAsync(staff);

        return Result.Success(true);
    }

    //[UnitOfWork]
    public async Task<Result<bool>> UpdateShiftZoneAssignmentsAsync(
    Guid shiftZoneId,
    ICollection<CreateUpdateZoneAssignmentDto> rows)
    {
        var shiftZone = await _shiftZoneRepository.FindAsync(shiftZoneId);
        if (shiftZone is null)
            return Result.Error("Shift zone not found.");

        var organizationId = shiftZone.OrganizationId;

        var query = await _shiftDetailsRepository.WithDetailsAsync(
            x => x.ShiftDevices,
            x => x.ShiftStaffDetails);

        var existingDetails = await AsyncExecuter.ToListAsync(
            query.Where(d => d.ShiftZoneId == shiftZoneId));

        var incomingIds = rows
            .Where(r => r.ExistingShiftDetailsId.HasValue && r.ExistingShiftDetailsId != Guid.Empty)
            .Select(r => r.ExistingShiftDetailsId!.Value)
            .ToHashSet();

        var toDelete = existingDetails
            .Where(d => !incomingIds.Contains(d.Id))
            .ToList();

        if (toDelete.Count > 0)
            await _shiftDetailsRepository.DeleteManyAsync(toDelete);

        var newRows = rows
            .Where(r => !r.ExistingShiftDetailsId.HasValue || r.ExistingShiftDetailsId == Guid.Empty)
            .ToList();

        var updateRows = rows
            .Where(r => r.ExistingShiftDetailsId.HasValue && r.ExistingShiftDetailsId != Guid.Empty)
            .ToList();

        if (newRows.Count > 0)
        {
            var toInsert = newRows.Select(_ => new ShiftDetails
            {
                ShiftZoneId = shiftZoneId,
                TenantId = shiftZone.TenantId,
            }).ToList();

            await _shiftDetailsRepository.InsertManyAsync(toInsert, autoSave: true);

            var newDevices = newRows
                .Zip(toInsert, (row, detail) => (row, detail))
                .Where(x => x.row.DeviceId is > 0)
                .Select(x => new ShiftDevice
                {
                    ShiftDetailsId = x.detail.Id,
                    DeviceId = x.row.DeviceId!.Value,
                    DeviceCode = x.row.DeviceCode,
                    DeviceUserName = x.row.DeviceUserName,
                    DevicePassword = x.row.DevicePassword ?? "P@ssw0rd",
                    TenantId = shiftZone.TenantId,
                }).ToList();

            var newStaff = newRows
                .Zip(toInsert, (row, detail) => (row, detail))
                .Where(x => x.row.StaffId is not null && x.row.StaffId != Guid.Empty)
                .Select(x => new ShiftStaffDetails
                {
                    ShiftDetailsId = x.detail.Id,
                    StaffId = x.row.StaffId!.Value,
                    StaffEnName = x.row.StaffEnName,
                    StaffArName = x.row.StaffUserName,
                    StaffPhoneNumber = x.row.StaffPhoneNumber,
                    TenantId = shiftZone.TenantId,
                }).ToList();

            if (newDevices.Any())
                await _shiftDeviceRepository.InsertManyAsync(newDevices);

            if (newStaff.Any())
                await _shiftStaffDetailsRepository.InsertManyAsync(newStaff);
        }

        if (updateRows.Count > 0)
        {
            var detailsMap = existingDetails.ToDictionary(d => d.Id);

            var missingId = updateRows.FirstOrDefault(r =>
                !detailsMap.ContainsKey(r.ExistingShiftDetailsId!.Value));
            if (missingId != null)
                return Result.Error($"ShiftDetails {missingId.ExistingShiftDetailsId} not found.");

            var pairs = updateRows
                .Select(row => (row, details: detailsMap[row.ExistingShiftDetailsId!.Value]))
                .ToList();

            var devicePairs = pairs.Where(p => p.row.DeviceId is > 0).ToList();

            var devicesToUpdate = devicePairs
                .Where(p => p.details.ShiftDevices != null && p.details.ShiftDevices.Id != Guid.Empty)
                .Select(p => p.details.ShiftDevices.ApplyUpdate(p.row, organizationId))
                .ToList();

            var devicesToInsert = devicePairs
                .Where(p => p.details.ShiftDevices == null || p.details.ShiftDevices.Id == Guid.Empty)
                .Select(p => p.details.ToNewDevice(p.row, organizationId))
                .ToList();

            var staffPairs = pairs.Where(p => p.row.StaffId is not null && p.row.StaffId != Guid.Empty).ToList();

            var staffToUpdate = staffPairs
                .Where(p => p.details.ShiftStaffDetails != null && p.details.ShiftStaffDetails.Id != Guid.Empty)
                .Select(p => p.details.ShiftStaffDetails.ApplyUpdate(p.row, organizationId))
                .ToList();

            var staffToInsert = staffPairs
                .Where(p => p.details.ShiftStaffDetails == null || p.details.ShiftStaffDetails.Id == Guid.Empty)
                .Select(p => p.details.ToNewStaff(p.row, organizationId))
                .ToList();

            if (devicesToInsert.Count > 0) await _shiftDeviceRepository.InsertManyAsync(devicesToInsert);
            if (devicesToUpdate.Count > 0) await _shiftDeviceRepository.UpdateManyAsync(devicesToUpdate);
            if (staffToInsert.Count > 0) await _shiftStaffDetailsRepository.InsertManyAsync(staffToInsert);
            if (staffToUpdate.Count > 0) await _shiftStaffDetailsRepository.UpdateManyAsync(staffToUpdate);
        }

        return Result.Success(true);
    }

}

/// <summary>
/// Extension helpers for shift assignment transformations.
/// Must be in a static class to be visible as extension methods.
/// </summary>
#region MappingExtensions
internal static class ShiftAssignmentExtensions
{
    public static ShiftDevice ApplyUpdate(this ShiftDevice device, CreateUpdateZoneAssignmentDto row, int organizationId)
    {
        device.DeviceId = row.DeviceId!.Value;
        device.DeviceCode = row.DeviceCode;
        device.DeviceUserName = row.DeviceUserName;
        device.DevicePassword = row.DevicePassword;
        return device;
    }

    public static ShiftDevice ToNewDevice(this ShiftDetails details, CreateUpdateZoneAssignmentDto row, int organizationId) =>
        new()
        {
            ShiftDetailsId = details.Id,
            DeviceId = row.DeviceId!.Value,
            DeviceCode = row.DeviceCode,
            DeviceUserName = row.DeviceUserName,
            DevicePassword = row.DevicePassword ?? "P@ssw0rd",
            TenantId = details.TenantId,
        };

    public static ShiftStaffDetails ApplyUpdate(this ShiftStaffDetails staff, CreateUpdateZoneAssignmentDto row, int organizationId)
    {
        staff.StaffId = row.StaffId!.Value;
        staff.StaffEnName = row.StaffEnName;
        staff.StaffArName = row.StaffUserName;
        staff.StaffPhoneNumber = row.StaffPhoneNumber;
        return staff;
    }

    public static ShiftStaffDetails ToNewStaff(this ShiftDetails details, CreateUpdateZoneAssignmentDto row, int organizationId) =>
        new()
        {
            ShiftDetailsId = details.Id,
            StaffId = row.StaffId!.Value,
            StaffEnName = row.StaffEnName,
            StaffArName = row.StaffUserName,
            StaffPhoneNumber = row.StaffPhoneNumber,
            TenantId = details.TenantId,
        };
#endregion
}



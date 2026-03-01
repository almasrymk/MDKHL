using SmartParking.Entities.Shifts;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Shifts.ShiftZones;

public class AssignShiftZoneAppService : ApplicationService, IAssignShiftZoneAppService, ITransientDependency
{
    private readonly IRepository<ShiftZone, Guid> _shiftZoneRepository;
    private readonly IRepository<ShiftDetails, Guid> _shiftDetailsRepository;
    private readonly IRepository<ShiftDevice, Guid> _shiftDeviceRepository;
    private readonly IRepository<ShiftStaffDetails, Guid> _shiftStaffDetailsRepository;

    public AssignShiftZoneAppService(
        IRepository<ShiftZone, Guid> shiftZoneRepository,
        IRepository<ShiftDetails, Guid> shiftDetailsRepository,
        IRepository<ShiftDevice, Guid> shiftDeviceRepository,
        IRepository<ShiftStaffDetails, Guid> shiftStaffDetailsRepository
    )
    {
        _shiftZoneRepository = shiftZoneRepository;
        _shiftDetailsRepository = shiftDetailsRepository;
        _shiftDeviceRepository = shiftDeviceRepository;
        _shiftStaffDetailsRepository = shiftStaffDetailsRepository;
    }

    public async Task<Result<bool>> AddShiftZoneAssignmentsAsync(Guid shiftZoneId, ICollection<CreateUpdateZoneAssignmentDto> rows)
    {
        var shiftZone = await _shiftZoneRepository.FindAsync(shiftZoneId);
        if (shiftZone is null)
            return Result.Error("Shift zone not found.");

        if (rows == null || rows.Count == 0)
            return Result.Success(true);

        var detailsToInsert = rows.Select(r => new ShiftDetails
        {
            ShiftZoneId = shiftZoneId,
            TenantId = shiftZone.TenantId,

            ShiftDevices = (r.DeviceId.HasValue && r.DeviceId > 0)
                ? new ShiftDevice((GuidGenerator.Create()))
                {
                    DeviceId = r.DeviceId.Value,
                    DeviceCode = r.DeviceCode,
                    DeviceUserName = r.DeviceUserName,
                    DevicePassword = r.DevicePassword ?? "P@ssw0rd",
                    TenantId = shiftZone.TenantId
                }
                : null,

            ShiftStaffDetails = (r.StaffId.HasValue && r.StaffId != Guid.Empty)
                ? new ShiftStaffDetails((GuidGenerator.Create()))
                {
                    StaffId = r.StaffId.Value,
                    StaffEnName = r.StaffEnName,
                    StaffArName = r.StaffUserName,
                    StaffPhoneNumber = r.StaffPhoneNumber,
                    TenantId = shiftZone.TenantId
                }
                : null

        }).ToList();

        await _shiftDetailsRepository.InsertManyAsync(detailsToInsert, autoSave: true);
        return Result.Success(true);
    }

    public async Task<Result<bool>> UpdateShiftZoneAssignmentsAsync(Guid shiftZoneId, ICollection<CreateUpdateZoneAssignmentDto> rows)
    {
        var shiftZone = await _shiftZoneRepository.FindAsync(shiftZoneId);
        if (shiftZone is null)
            return Result.Error("Shift zone not found.");

        var tenantId = shiftZone.TenantId;

        var incomingExistingIds = rows
            .Where(r => r.ExistingShiftDetailsId.HasValue && r.ExistingShiftDetailsId != Guid.Empty)
            .Select(r => r.ExistingShiftDetailsId!.Value)
            .ToList();


        var detailsToDelete = await _shiftDetailsRepository.GetListAsync(
            d => d.ShiftZoneId == shiftZoneId && !incomingExistingIds.Contains(d.Id)
        );
        if (detailsToDelete.Any())
        {
            await _shiftDetailsRepository.DeleteManyAsync(detailsToDelete);
        }

        var newDetails = rows
            .Where(r => !r.ExistingShiftDetailsId.HasValue || r.ExistingShiftDetailsId == Guid.Empty)
            .Select(r => new ShiftDetails
            {
                ShiftZoneId = shiftZoneId,
                TenantId = tenantId,

                ShiftDevices = (r.DeviceId.HasValue && r.DeviceId > 0)
                    ? new ShiftDevice((GuidGenerator.Create()))
                    {
                        DeviceId = r.DeviceId.Value,
                        DeviceCode = r.DeviceCode,
                        DeviceUserName = r.DeviceUserName,
                        DevicePassword = r.DevicePassword ?? "P@ssw0rd",
                        TenantId = tenantId
                    }
                    : null,

                ShiftStaffDetails = (r.StaffId.HasValue && r.StaffId != Guid.Empty)
                    ? new ShiftStaffDetails((GuidGenerator.Create()))
                    {
                        StaffId = r.StaffId.Value,
                        StaffEnName = r.StaffEnName,
                        StaffArName = r.StaffUserName,
                        StaffPhoneNumber = r.StaffPhoneNumber,
                        TenantId = tenantId
                    }
                    : null

            }).ToList();

        if (newDetails.Any())
            await _shiftDetailsRepository.InsertManyAsync(newDetails, autoSave: true);

        var updateIds = incomingExistingIds;

        var devicesToDelete = await _shiftDeviceRepository.GetListAsync(
            d => d.ShiftDetailsId.HasValue && updateIds.Contains(d.ShiftDetailsId.Value)
        );
        if(devicesToDelete.Any())
        {
            await _shiftDeviceRepository.DeleteManyAsync(devicesToDelete);
        }

        var staffToDelete = await _shiftStaffDetailsRepository.GetListAsync(
            s => s.ShiftDetailsId.HasValue && updateIds.Contains(s.ShiftDetailsId.Value)
        );
        if(staffToDelete.Any())
        {
            await _shiftStaffDetailsRepository.DeleteManyAsync(staffToDelete);
        }

        var devicesToInsert = rows
            .Where(r => r.ExistingShiftDetailsId.HasValue && r.DeviceId.HasValue && r.DeviceId > 0)
            .Select(r => new ShiftDevice((GuidGenerator.Create()))
            {
                ShiftDetailsId = r.ExistingShiftDetailsId!.Value,
                DeviceId = r.DeviceId!.Value,
                DeviceCode = r.DeviceCode,
                DeviceUserName = r.DeviceUserName,
                DevicePassword = r.DevicePassword ?? "P@ssw0rd",
                TenantId = tenantId
            }).ToList();

        if (devicesToInsert.Any())
            await _shiftDeviceRepository.InsertManyAsync(devicesToInsert, autoSave: false);

        var staffToInsert = rows
            .Where(r => r.ExistingShiftDetailsId.HasValue && r.StaffId.HasValue && r.StaffId != Guid.Empty)
            .Select(r => new ShiftStaffDetails((GuidGenerator.Create()))
            {
                ShiftDetailsId = r.ExistingShiftDetailsId!.Value,
                StaffId = r.StaffId!.Value,
                StaffEnName = r.StaffEnName,
                StaffArName = r.StaffUserName,
                StaffPhoneNumber = r.StaffPhoneNumber,
                TenantId = tenantId
            }).ToList();

        if (staffToInsert.Any())
            await _shiftStaffDetailsRepository.InsertManyAsync(staffToInsert, autoSave: false);

        await CurrentUnitOfWork.SaveChangesAsync();
        return Result.Success(true);
    }
}
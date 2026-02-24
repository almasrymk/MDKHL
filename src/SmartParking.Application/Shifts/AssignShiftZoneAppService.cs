using SmartParking.Entities.Shifts;
using SmartParking.Results;
using SmartParking.Shifts.ShiftDetail;
using SmartParking.Shifts.ShiftDevices;
using SmartParking.Shifts.ShiftStaffDetail;
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
    private readonly IShiftStaffDetailsAppService _shiftStaffDetailsAppService;
    private readonly IShiftDetailsAppService _shiftDetailsAppService;
    private readonly IShiftDeviceAppService _shiftDeviceAppService;
    private readonly IRepository<ShiftDevice, Guid> _shiftDeviceRepository;
    private readonly IRepository<ShiftDetails, Guid> _shiftDetailsRepository;
    private readonly IRepository<ShiftStaffDetails, Guid> _shiftStaffDetailsRepository;
    public AssignShiftZoneAppService(IRepository<ShiftZone, Guid> shiftZoneRepository,
        IShiftStaffDetailsAppService shiftStaffDetailsAppService,
        IShiftDetailsAppService shiftDetailsAppService,
        IShiftDeviceAppService shiftDeviceAppService,
        IRepository<ShiftDetails, Guid> shiftDetailsRepository,
        IRepository<ShiftStaffDetails, Guid> shiftStaffDetailsRepository,
        IRepository<ShiftDevice, Guid> shiftDeviceRepository)
    {
        _shiftZoneRepository = shiftZoneRepository;
        _shiftStaffDetailsAppService = shiftStaffDetailsAppService;
        _shiftDetailsAppService = shiftDetailsAppService;
        _shiftDeviceAppService = shiftDeviceAppService;
        _shiftDeviceRepository = shiftDeviceRepository;
        _shiftStaffDetailsRepository = shiftStaffDetailsRepository;
        _shiftDetailsRepository = shiftDetailsRepository;
    }

    public async Task<Result<bool>> AssignDeviceToShiftZone(Guid shiftZoneId, ICollection<CreateUpdateShiftDeviceDto> dtos)
    {
        var query = await _shiftZoneRepository.WithDetailsAsync(x => x.ShiftDetails);
        var shiftZone = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == shiftZoneId));

        if (shiftZone == null)
            return Result.Error("Shift zone not found");

        if (shiftZone.ShiftDetails == null)
        {
            var created = await _shiftDetailsAppService.CreateAsync(new CreateUpdateShiftDetailsDto());
            shiftZone.ShiftDetails = await _shiftDetailsRepository.GetAsync(created.Id);
            shiftZone.ShiftDetailsId = created.Id;
        }

        foreach (var dto in dtos)
            dto.ShiftDetailsId = shiftZone.ShiftDetailsId;

        var createdIds = await _shiftDeviceAppService.CreateShiftDeviceListAsync(dtos.ToList());
        var createdEntities = await _shiftDeviceRepository.GetListAsync(x => createdIds.Contains(x.Id));

        shiftZone.ShiftDetails.ShiftDevices = createdEntities;

        await _shiftZoneRepository.UpdateAsync(shiftZone, autoSave: true);

        return Result.Success(true);
    }

    public async Task<Result<bool>> AssignStaffToShiftZone(Guid shiftZoneId, ICollection<CreateUpdateShiftStaffDetailsDto> dtos)
    {
        var query = await _shiftZoneRepository.WithDetailsAsync(x => x.ShiftDetails);
        var shiftZone = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == shiftZoneId));

        if (shiftZone == null)
            return Result.Error("Shift zone not found");

        if (shiftZone.ShiftDetails == null)
        {
            var created = await _shiftDetailsAppService.CreateAsync(new CreateUpdateShiftDetailsDto());
            shiftZone.ShiftDetails = await _shiftDetailsRepository.GetAsync(created.Id);
            shiftZone.ShiftDetailsId = created.Id;
        }

        foreach (var dto in dtos)
            dto.ShiftDetailsId = shiftZone.ShiftDetailsId;

        var createdIds = await _shiftStaffDetailsAppService.CreateShiftStaffListAsync(dtos.ToList());
        var createdEntities = await _shiftStaffDetailsRepository.GetListAsync(x => createdIds.Contains(x.Id));

        shiftZone.ShiftDetails.ShiftStaffDetails = createdEntities;

        await _shiftZoneRepository.UpdateAsync(shiftZone, autoSave: true);

        return Result.Success(true);
    }

    public async Task<Result<bool>> SyncDevicesForShiftZone(Guid shiftZoneId, ICollection<CreateUpdateShiftDeviceDto> incomingDtos)
    {
        var query = await _shiftZoneRepository.WithDetailsAsync(x => x.ShiftDetails!.ShiftDevices);
        var shiftZone = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == shiftZoneId));

        if (shiftZone == null)
            return Result.Error("Shift zone not found");

        if (shiftZone.ShiftDetails == null)
        {
            var created = await _shiftDetailsAppService.CreateAsync(new CreateUpdateShiftDetailsDto());
            shiftZone.ShiftDetails = await _shiftDetailsRepository.GetAsync(created.Id);
            shiftZone.ShiftDetailsId = created.Id;
        }

        var current = shiftZone.ShiftDetails.ShiftDevices ?? new List<ShiftDevice>();
        var currentById = current.ToDictionary(d => d.Id);

        foreach (var dto in incomingDtos.Where(x => x.Id.HasValue))
        {
            if (currentById.TryGetValue(dto.Id.Value, out var entity))
            {
                entity.DeviceId = dto.DeviceId;
                entity.DeviceCode = dto.DeviceCode;
                entity.DeviceUserName = dto.DeviceUserName;
                entity.DevicePassword = dto.DevicePassword;

                await _shiftDeviceRepository.UpdateAsync(entity, autoSave: false);
            }
        }

        var createdIdSet = new HashSet<Guid>();

        var toCreate = incomingDtos.Where(x => !x.Id.HasValue).ToList();
        if (toCreate.Any())
        {
            foreach (var c in toCreate)
                c.ShiftDetailsId = shiftZone.ShiftDetailsId;

            var createdIds = await _shiftDeviceAppService.CreateShiftDeviceListAsync(toCreate);
            var createdEntities = await _shiftDeviceRepository.GetListAsync(x => createdIds.Contains(x.Id));
            createdIdSet = createdEntities.Select(x => x.Id).ToHashSet(); // ← track them
            current.AddRange(createdEntities);
        }

        var keepIds = incomingDtos.Where(x => x.Id.HasValue).Select(x => x.Id!.Value).ToHashSet();

        var toRemove = current
            .Where(x => !keepIds.Contains(x.Id) && !createdIdSet.Contains(x.Id))
            .ToList();

        foreach (var removed in toRemove)
        {
            current.Remove(removed);
            await _shiftDeviceRepository.DeleteAsync(removed.Id, autoSave: false);
        }

        shiftZone.ShiftDetails.ShiftDevices = current;

        await _shiftZoneRepository.UpdateAsync(shiftZone, autoSave: true);

        return Result.Success(true);
    }

    public async Task<Result<bool>> SyncStaffForShiftZone(Guid shiftZoneId, ICollection<CreateUpdateShiftStaffDetailsDto> incomingDtos)
    {
        var query = await _shiftZoneRepository.WithDetailsAsync(x => x.ShiftDetails!.ShiftStaffDetails);
        var shiftZone = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == shiftZoneId));

        if (shiftZone == null)
            return Result.Error("Shift zone not found");

        if (shiftZone.ShiftDetails == null)
        {
            var created = await _shiftDetailsAppService.CreateAsync(new CreateUpdateShiftDetailsDto());
            shiftZone.ShiftDetails = await _shiftDetailsRepository.GetAsync(created.Id);
            shiftZone.ShiftDetailsId = created.Id;
        }

        var current = shiftZone.ShiftDetails.ShiftStaffDetails ?? new List<ShiftStaffDetails>();
        var currentById = current.ToDictionary(d => d.Id);

        foreach (var dto in incomingDtos.Where(x => x.Id.HasValue))
        {
            if (currentById.TryGetValue(dto.Id.Value, out var entity))
            {
                entity.StaffId = dto.StaffId;
                entity.StaffArName = dto.StaffUserName;
                entity.StaffEnName = dto.StaffEnName;
                entity.StaffPhoneNumber = dto.StaffPhoneNumber;

                await _shiftStaffDetailsRepository.UpdateAsync(entity, autoSave: false);
            }
        }
        var createdIdSet = new HashSet<Guid>();

        var toCreate = incomingDtos.Where(x => !x.Id.HasValue).ToList();
        if (toCreate.Any())
        {
            foreach (var c in toCreate)
                c.ShiftDetailsId = shiftZone.ShiftDetailsId;

            var createdIds = await _shiftStaffDetailsAppService.CreateShiftStaffListAsync(toCreate);
            var createdEntities = await _shiftStaffDetailsRepository.GetListAsync(x => createdIds.Contains(x.Id));
            createdIdSet = createdEntities.Select(x => x.Id).ToHashSet(); // ← track them
            current.AddRange(createdEntities);
        }

        var keepIds = incomingDtos.Where(x => x.Id.HasValue).Select(x => x.Id!.Value).ToHashSet();

        var toRemove = current
            .Where(x => !keepIds.Contains(x.Id) && !createdIdSet.Contains(x.Id))
            .ToList();

        foreach (var removed in toRemove)
        {
            current.Remove(removed);
            await _shiftStaffDetailsRepository.DeleteAsync(removed.Id, autoSave: false);
        }

        shiftZone.ShiftDetails.ShiftStaffDetails = current;

        await _shiftZoneRepository.UpdateAsync(shiftZone, autoSave: true);

        return Result.Success(true);
    }
}
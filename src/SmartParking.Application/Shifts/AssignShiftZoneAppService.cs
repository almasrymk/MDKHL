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
    private readonly IShiftDeviceAppService _shiftDeviceAppService;
    private readonly IRepository<ShiftDevice, Guid> _shiftDeviceRepository;
    private readonly IRepository<ShiftDetails, Guid> _shiftDetailsRepository;
    private readonly IRepository<ShiftStaffDetails, Guid> _shiftStaffDetailsRepository;
    public AssignShiftZoneAppService(IRepository<ShiftZone, Guid> shiftZoneRepository,
        IShiftStaffDetailsAppService shiftStaffDetailsAppService,
        IShiftDeviceAppService shiftDeviceAppService,
        IRepository<ShiftDetails, Guid> shiftDetailsRepository,
        IRepository<ShiftStaffDetails, Guid> shiftStaffDetailsRepository,
        IRepository<ShiftDevice, Guid> shiftDeviceRepository)
    {
        _shiftZoneRepository = shiftZoneRepository;
        _shiftStaffDetailsAppService = shiftStaffDetailsAppService;
        _shiftDeviceAppService = shiftDeviceAppService;
        _shiftDeviceRepository = shiftDeviceRepository;
        _shiftStaffDetailsRepository = shiftStaffDetailsRepository;
        _shiftDetailsRepository = shiftDetailsRepository;
    }

    

    public async Task<Result<bool>> SyncShiftZoneAssignments(
    Guid shiftZoneId,
    ICollection<SyncShiftZoneAssignmentDto> rows)
    {
        var query = await _shiftZoneRepository.WithDetailsAsync(x => x.ShiftDetails);
        var shiftZone = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == shiftZoneId));
        if (shiftZone == null) return Result.Error("Shift zone not found");

        shiftZone.ShiftDetails ??= new List<ShiftDetails>();

        foreach (var row in rows)
        {
            ShiftDetails details;

            if (row.ExistingShiftDetailsId.HasValue && row.ExistingShiftDetailsId != Guid.Empty)
            {
                details = shiftZone.ShiftDetails
                    .SingleOrDefault(x => x.Id == row.ExistingShiftDetailsId.Value);

                if (details == null) return Result.Error($"ShiftDetails {row.ExistingShiftDetailsId} not found");
            }
            else
            {
                
                details = new ShiftDetails
                {
                    ShiftZoneId = shiftZoneId,
                    TenantId = shiftZone.TenantId
                };
                await _shiftDetailsRepository.InsertAsync(details, autoSave: true);
                shiftZone.ShiftDetails.Add(details);
            }

            
            if (row.DeviceId.HasValue && row.DeviceId > 0)
            {
                if (details.ShiftDevices != null && details.ShiftDevices.Id != Guid.Empty)
                {
                    details.ShiftDevices.DeviceId = row.DeviceId.Value;
                    details.ShiftDevices.DeviceCode = row.DeviceCode;
                    details.ShiftDevices.DeviceUserName = row.DeviceUserName;
                    details.ShiftDevices.DevicePassword = row.DevicePassword;
                    await _shiftDeviceRepository.UpdateAsync(details.ShiftDevices);
                }
                else
                {
                    var deviceDto = new CreateUpdateShiftDeviceDto
                    {
                        ShiftDetailsId = details.Id,
                        DeviceId = row.DeviceId.Value,
                        DeviceCode = row.DeviceCode,
                        DeviceUserName = row.DeviceUserName,
                        DevicePassword = row.DevicePassword ?? "P@ssw0rd"
                    };
                    var createdDeviceId = (await _shiftDeviceAppService
                        .CreateShiftDeviceListAsync(new List<CreateUpdateShiftDeviceDto> { deviceDto }))
                        .FirstOrDefault();
                    details.ShiftDevices = await _shiftDeviceRepository.GetAsync(createdDeviceId);
                }
            }

            
            if (row.StaffId.HasValue && row.StaffId != Guid.Empty)
            {
                if (details.ShiftStaffDetails != null && details.ShiftStaffDetails.Id != Guid.Empty)
                {
                    details.ShiftStaffDetails.StaffId = row.StaffId.Value;
                    details.ShiftStaffDetails.StaffEnName = row.StaffEnName;
                    details.ShiftStaffDetails.StaffArName = row.StaffUserName;
                    details.ShiftStaffDetails.StaffPhoneNumber = row.StaffPhoneNumber;
                    await _shiftStaffDetailsRepository.UpdateAsync(details.ShiftStaffDetails);
                }
                else
                {
                    var staffDto = new CreateUpdateShiftStaffDetailsDto
                    {
                        ShiftDetailsId = details.Id,
                        StaffId = row.StaffId.Value,
                        StaffUserName = row.StaffUserName,
                        StaffEnName = row.StaffEnName,
                        StaffPhoneNumber = row.StaffPhoneNumber
                    };
                    var createdStaffId = (await _shiftStaffDetailsAppService
                        .CreateShiftStaffListAsync(new List<CreateUpdateShiftStaffDetailsDto> { staffDto }))
                        .FirstOrDefault();
                    details.ShiftStaffDetails = await _shiftStaffDetailsRepository.GetAsync(createdStaffId);
                }
            }
        }

        await _shiftZoneRepository.UpdateAsync(shiftZone, autoSave: true);
        return Result.Success(true);
    }
}
using SmartParking.Results;
using SmartParking.Shifts.ShiftStaffDetail;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Shifts.ShiftDevices;

public interface IShiftDeviceAppService 
    : ICrudAppService<ShiftDeviceDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateShiftDeviceDto>
{
    //Task<Result<bool>> CreateShiftDeviceListAsync(List<CreateUpdateShiftDeviceDto> dtos);
    Task<List<Guid>> CreateShiftDeviceListAsync(List<CreateUpdateShiftDeviceDto> dtos);

}

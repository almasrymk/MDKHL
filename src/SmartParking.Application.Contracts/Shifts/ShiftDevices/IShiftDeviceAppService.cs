using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Shifts.ShiftDevices;

public interface IShiftDeviceAppService 
    : ICrudAppService<ShiftDeviceDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateShiftDeviceDto>
{
    /// <summary>
    /// Creates multiple <see cref="ShiftDevice"/> records in bulk.
    /// </summary>
    /// <param name="dtos">List of <see cref="CreateUpdateShiftDeviceDto"/> to insert.</param>
    /// <returns>A list of created <see cref="ShiftDeviceDto"/> with all populated fields including generated IDs.</returns>
    Task<List<ShiftDeviceDto>> CreateShiftDeviceListAsync(List<CreateUpdateShiftDeviceDto> dtos);
}
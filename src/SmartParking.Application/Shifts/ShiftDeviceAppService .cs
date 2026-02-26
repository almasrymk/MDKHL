using SmartParking.Entities.Shifts;
using SmartParking.Shifts.ShiftDevices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Shifts;

public class ShiftDeviceAppService
    : CrudAppService<ShiftDevice, ShiftDeviceDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateShiftDeviceDto>,
    IShiftDeviceAppService
{
    private readonly IRepository<ShiftDevice, Guid> _repository;

    public ShiftDeviceAppService(IRepository<ShiftDevice, Guid> repository) : base(repository)
    {
        _repository = repository;
    }
    /// <inheritdoc/>
    public async Task<List<ShiftDeviceDto>> CreateShiftDeviceListAsync(List<CreateUpdateShiftDeviceDto> dtos)
    {
        var entities = ObjectMapper.Map<List<CreateUpdateShiftDeviceDto>, List<ShiftDevice>>(dtos);
        await _repository.InsertManyAsync(entities, autoSave: true);
        return ObjectMapper.Map<List<ShiftDevice>, List<ShiftDeviceDto>>(entities);
    }
}
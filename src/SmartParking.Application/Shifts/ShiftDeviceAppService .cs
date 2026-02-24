using SmartParking.Entities.Shifts;
using SmartParking.Results;
using SmartParking.Shifts.ShiftDevices;
using SmartParking.Shifts.ShiftStaffDetail;
using System;
using System.Collections.Generic;
using System.Linq;
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

    //public async Task<Result<bool>> CreateShiftDeviceListAsync(List<CreateUpdateShiftDeviceDto> dtos)
    //{
    //    var entites = ObjectMapper.Map<List<CreateUpdateShiftDeviceDto>, List<ShiftDevice>>(dtos);

    //    try
    //    {
    //        await _repository.InsertManyAsync(entites, autoSave: true);
    //    }
    //    catch (Exception ex)
    //    {
    //        return Result.Error("Unable to insert the data");
    //    }

    //    return Result.Success(true);
    //}

    public async Task<List<Guid>> CreateShiftDeviceListAsync(List<CreateUpdateShiftDeviceDto> dtos)
    {
        var entities = ObjectMapper.Map<List<CreateUpdateShiftDeviceDto>, List<ShiftDevice>>(dtos);

        await _repository.InsertManyAsync(entities, autoSave: true);

        return entities.Select(x => x.Id).ToList();
    }

}

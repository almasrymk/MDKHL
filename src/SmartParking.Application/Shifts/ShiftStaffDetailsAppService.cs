using SmartParking.Entities.Shifts;
using SmartParking.Shifts.ShiftStaffDetail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Shifts;

public class ShiftStaffDetailsAppService : CrudAppService<
    ShiftStaffDetails,
    ShiftStaffDetailsDto,
    Guid, 
    PagedAndSortedResultRequestDto, 
    CreateUpdateShiftStaffDetailsDto>,
    IShiftStaffDetailsAppService
{
    private readonly IRepository<ShiftStaffDetails, Guid> _repository;
    public ShiftStaffDetailsAppService(IRepository<ShiftStaffDetails, Guid> repository) : base(repository)
    {
        _repository = repository;
    }

    //public async Task<Result<bool>> CreateShiftStaffListAsync(List<CreateUpdateShiftStaffDetailsDto> dtos)
    //{
    //    var entites = ObjectMapper.Map<List<CreateUpdateShiftStaffDetailsDto>, List<ShiftStaffDetails>>(dtos);

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

    public async Task<List<Guid>> CreateShiftStaffListAsync(List<CreateUpdateShiftStaffDetailsDto> dtos)
    {
        var entities = ObjectMapper.Map<List<CreateUpdateShiftStaffDetailsDto>, List<ShiftStaffDetails>>(dtos);

        await _repository.InsertManyAsync(entities, autoSave: true);

        return entities.Select(x => x.Id).ToList();
    }

}

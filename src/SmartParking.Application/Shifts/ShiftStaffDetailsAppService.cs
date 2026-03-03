using SmartParking.Entities.Shifts;
using SmartParking.Shifts.ShiftStaffDetail;
using System;
using System.Collections.Generic;
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
    /// <inheritdoc/>
    public async Task<List<ShiftStaffDetailsDto>> CreateShiftStaffListAsync(List<CreateUpdateShiftStaffDetailsDto> dtos)
    {
        var entities = ObjectMapper.Map<List<CreateUpdateShiftStaffDetailsDto>, List<ShiftStaffDetails>>(dtos);
        await _repository.InsertManyAsync(entities, autoSave: true);
        return ObjectMapper.Map<List<ShiftStaffDetails>, List<ShiftStaffDetailsDto>>(entities);
    }
}
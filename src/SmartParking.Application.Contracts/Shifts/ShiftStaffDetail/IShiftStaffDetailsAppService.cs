using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Shifts.ShiftStaffDetail;

public interface IShiftStaffDetailsAppService : 
    ICrudAppService<ShiftStaffDetailsDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateShiftStaffDetailsDto>
{
    //Task<Result<bool>> CreateShiftStaffListAsync(List<CreateUpdateShiftStaffDetailsDto> dtos);
    Task<List<Guid>> CreateShiftStaffListAsync(List<CreateUpdateShiftStaffDetailsDto> dtos);

}
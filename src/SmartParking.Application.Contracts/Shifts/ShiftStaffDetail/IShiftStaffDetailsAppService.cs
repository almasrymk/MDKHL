using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Shifts.ShiftStaffDetail;

public interface IShiftStaffDetailsAppService : 
    ICrudAppService<ShiftStaffDetailsDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateShiftStaffDetailsDto>
{
    /// <summary>
    /// Creates multiple <see cref="ShiftStaffDetails"/> records in bulk.
    /// </summary>
    /// <param name="dtos">List of <see cref="CreateUpdateShiftStaffDetailsDto"/> to insert.</param>
    /// <returns>A list of created <see cref="ShiftStaffDetailsDto"/> with all populated fields including generated IDs.</returns>
    Task<List<ShiftStaffDetailsDto>> CreateShiftStaffListAsync(List<CreateUpdateShiftStaffDetailsDto> dtos);
}
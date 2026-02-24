using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Shifts.ShiftDetail;

public interface IShiftDetailsAppService : ICrudAppService<ShiftDetailsDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateShiftDetailsDto>
{
    Task<ShiftDetailsDto?> GetAllListAsync();
    Task<ShiftDetailsDto?> GetByShiftZoneAsync(Guid shiftZoneId);
}
using SmartParking.Cameras;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.DailyShifts;
public interface IDailyShiftAppService :
     ICrudAppService<
        DailyShiftDto,
        int,
        PagedAndSortedAndSearchResultRequestDto,
        CreateUpdateDailyShiftDto
        >
{
    Task<DailyShiftDto> CreateAsync(CreateUpdateDailyShiftDto dto);
    Task<DailyShiftDto> UpdateAsync(int id, CreateUpdateDailyShiftDto dto);
    Task DeleteAsync(int id);
    Task<DailyShiftDto> GetAsync(int id);
    Task<PagedResultDto<DailyShiftDto>> GetListDailyShiftsAsync(PagedAndSortedAndSearchResultRequestDto input,Guid? TenantId, Guid? userId);
    Task<PagedResultDto<DailyShiftDto>> GetListDailyShiftsOrganizationAsync(PagedAndSortedAndSearchResultRequestDto input, Guid? TenantId , Guid? userId );
}

using SmartParking.Cameras;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Shifts;

public interface IShiftAppService : ICrudAppService<ShiftDto,int,PagedAndSortedResultRequestDto,CreateUpdateShiftDto>
{

    Task<ShiftDto> CreateForMDKHLAsync(CreateUpdateShiftDto shift); 
    Task<ShiftDto> CreateAsync(CreateUpdateShiftDto shift);
    Task<PagedResultDto<ShiftDto>> GetListOrganizationAsync(PagedAndSortedAndSearchResultRequestDto input);
    Task<PagedResultDto<ShiftDto>> GetListMdkhlAsync(PagedAndSortedAndSearchResultRequestDto input, Guid? tenantId);
    Task<ShiftDto> UpdateAsync(int id, CreateUpdateShiftDto input);

    Task<ShiftDto> GetShiftAsync(int id, Guid? tenantId);
    


}

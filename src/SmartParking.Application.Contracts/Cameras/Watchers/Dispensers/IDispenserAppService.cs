using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Cameras.Watchers.Dispensers;

public interface IDispenserAppService : ICrudAppService<DispenserDto,int, PagedAndSortedResultRequestDto, CreateUpdateDispenserDto>
{
    Task<PagedResultDto<DispenserDto>> GetAllAsync(PagedAndSortedAndSearchResultRequestDto input, Guid? tenantId);
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.StateManagements;

public interface IStateManagementAppService : ICrudAppService<StateManagementDto, int, PagedAndSortedResultRequestDto, CreateUpdateStateManagementDto>
{
   Task<StateManagementDto> GetStateManagementByUserAsync(Guid? userId);
}

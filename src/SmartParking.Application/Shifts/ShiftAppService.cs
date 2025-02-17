using Microsoft.AspNetCore.Authorization;
using SmartParking.Entities.Shifts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Shifts;

[Authorize]
 
public class ShiftAppService : CrudAppService<Shift, ShiftDto, int, PagedAndSortedResultRequestDto, CreateUpdateShiftDto>, IShiftAppService
{
    public ShiftAppService(IRepository<Shift, int> repository) : base(repository)
    {
    }
}

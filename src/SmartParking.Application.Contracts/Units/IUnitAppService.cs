using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Units;

public interface IUnitAppService : ICrudAppService<
    UnitDto,
    int,
    PagedAndSortedResultRequestDto,
    CreateUpdateUnitDto>
{
}

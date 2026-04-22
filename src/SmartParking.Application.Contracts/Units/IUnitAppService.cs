using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Units;

public interface IUnitAppService : ICrudAppService<
    UnitDto,
    int,
    PagedAndSortedResultRequestDto,
    CreateUpdateUnitDto>
{
    Task<List<UnitDto>> GetAllActiveUnitsAsync();
    Task<Result<UnitDto>> GetActiveUnitByCodeAsync(string unitCode);
}

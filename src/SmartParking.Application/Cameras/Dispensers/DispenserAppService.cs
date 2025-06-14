using Microsoft.AspNetCore.Mvc;
using SmartParking.Cameras.Watchers.Dispensers;
using SmartParking.Entities.Cameras.Watchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Cameras.Dispensers;

public class DispenserAppService : CrudAppService<Dispenser, DispenserDto, int, PagedAndSortedResultRequestDto, CreateUpdateDispenserDto>, IDispenserAppService
{
    private readonly IRepository<Dispenser, int> _repository;
    private readonly IDataFilter _dataFilter;

    public DispenserAppService(IRepository<Dispenser, int> repository, IDataFilter dataFilter) : base(repository)
    {
        _repository = repository;
        _dataFilter = dataFilter;
    }

    [Route("GetAllDispensers")]
    public async Task<PagedResultDto<DispenserDto>> GetAllAsync(PagedAndSortedAndSearchResultRequestDto input, Guid? tenantId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var queryable = await _repository.GetQueryableAsync();
            queryable = ApplyTenant(tenantId, queryable);
            queryable = ApplySearch(input, queryable);
            queryable = ApplySort(input, queryable);
            var totalCount = await AsyncExecuter.CountAsync(queryable);
            List<Dispenser> dispensers = await ApplyPaged(input, queryable);
            var dispensersDto = ObjectMapper.Map<List<Dispenser>, List<DispenserDto>>(dispensers);
            return new PagedResultDto<DispenserDto>(
                totalCount,
                dispensersDto
            );
        }
    }


    private static IQueryable<Dispenser> ApplySort(PagedAndSortedAndSearchResultRequestDto input, IQueryable<Dispenser> queryable)
    {
        if (!input.Sorting.IsNullOrEmpty())
        {
            if (input.Sorting.Contains("DESC"))
            {
                var sorting = input.Sorting.Split(' ').ToList();
                queryable = queryable.OrderBy(sorting[0] + " descending");
            }
            else
            {
                queryable = queryable.OrderBy(input.Sorting);
            }
        }
        return queryable;
    }


    private async Task<List<Dispenser>> ApplyPaged(PagedAndSortedAndSearchResultRequestDto input, IQueryable<Dispenser> queryable)
    {
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        var cameras = await AsyncExecuter.ToListAsync(queryable);
        return cameras;
    }

    private static IQueryable<Dispenser> ApplySearch(PagedAndSortedAndSearchResultRequestDto input, IQueryable<Dispenser> queryable)
    {
        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {
            queryable = queryable.Where(or =>
                or.Name.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.DeviceId.ToLower().Contains(input.SearchingTerm.ToLower()) 
                           );
        }
        return queryable;
    }

    private static IQueryable<Dispenser> ApplyTenant(Guid? tenantId, IQueryable<Dispenser> queryable)
    {
        if (tenantId.HasValue)
            queryable = queryable.Where(_ => _.TenantId == tenantId);
        return queryable;
    }
}

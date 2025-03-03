using SmartParking.Entities.Discounts;
using SmartParking.Entities.Organizations;
using SmartParking.Filters;
using SmartParking.Garages;
using SmartParking.OrganizationsGarages;
using SmartParking.Packages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;

namespace SmartParking.Discounts;

public class DiscountAppService : CrudAppService<Discount, DiscountDto, int, PagedAndSortedResultRequestDto, CreateUpdateDiscountDto>, IDiscountAppService
{
    private readonly IRepository<Discount, int> _repository;

    public DiscountAppService(IRepository<Discount, int> repository) : base(repository)
    {
        _repository = repository;
    }

    public async Task<PagedResultDto<DiscountDto>> GetListAsync(PagedAndSortedAndSearchResultRequestDto input)
    {
        var query = await _repository.GetQueryableAsync();
        query = SearchCriteria(input, query);
        var totalCount = await AsyncExecuter.CountAsync(query);
        query = query.Skip(input.SkipCount).Take(input.MaxResultCount);
        var discounts = await AsyncExecuter.ToListAsync(query);
        var discountsDto = ObjectMapper.Map<List<Discount>, List<DiscountDto>>(discounts);
        return new PagedResultDto<DiscountDto>(
            totalCount,
            discountsDto
        );
    }

    private static IQueryable<Discount> SearchCriteria(PagedAndSortedAndSearchResultRequestDto input, IQueryable<Discount> queryable)
    {
        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {

            queryable = queryable.Where(_ =>
                _.ArabicName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                _.EnglishName.ToLower().Contains(input.SearchingTerm.ToLower()) || 
                _.Status.Equals(input.SearchingTerm.ToLower()) || 
                _.DiscountPercent.ToString().Contains(input.SearchingTerm.ToLower())
            );
        }

        return queryable;
    }
}

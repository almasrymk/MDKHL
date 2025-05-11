using Microsoft.AspNetCore.Mvc;
using SmartParking.Entities.Discounts;
using SmartParking.Entities.Organizations;
using SmartParking.Filters;
using SmartParking.Garages;
using SmartParking.OrganizationsGarages;
using SmartParking.Packages;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;

namespace SmartParking.Discounts;

public class DiscountAppService : CrudAppService<Discount, DiscountDto, int, PagedAndSortedAndSearchResultRequestDto, CreateUpdateDiscountDto>, IDiscountAppService
{
    private readonly IRepository<Discount, int> _repository;

    public DiscountAppService(IRepository<Discount, int> repository) : base(repository)
    {
        _repository = repository;
    }
    #region Public Methods
    public override async Task<PagedResultDto<DiscountDto>> GetListAsync(PagedAndSortedAndSearchResultRequestDto input)
    {
        var query = await _repository.GetQueryableAsync();
        query = ApplySort(input, query);
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

   

    public async Task<Result<List<DiscountDto>>> GetListMobileAsync()
    {
        var query = await _repository.GetQueryableAsync();
        var discounts = await AsyncExecuter.ToListAsync(query);
        var discountsDto = ObjectMapper.Map<List<Discount>, List<DiscountDto>>(discounts);
        return Result<List<DiscountDto>>.Success(discountsDto);
    }

   

    public async Task<DiscountDto> DeleteDiscountAsync(int id)
    {
        var discount = await _repository.FirstOrDefaultAsync(_ => _.Id == id);
        var discountsDto = ObjectMapper.Map<Discount,DiscountDto>(discount);
        await _repository.HardDeleteAsync(discount);
        return discountsDto;
    }
    #endregion
    #region Private Methods
    private static IQueryable<Discount> SearchCriteria(PagedAndSortedAndSearchResultRequestDto input, IQueryable<Discount> queryable)
    {
        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {

            queryable = queryable.Where(_ =>
                _.ArabicName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                _.EnglishName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                _.Status.ToString().ToLower().Contains(input.SearchingTerm.ToLower()) ||
                _.DiscountPercent.ToString().Contains(input.SearchingTerm.ToLower())
            );
        }

        return queryable;
    }
    private static IQueryable<Discount>? ApplySort(PagedAndSortedAndSearchResultRequestDto input, IQueryable<Discount>? query)
    {
        var sorting = input.Sorting;
        if (!string.IsNullOrEmpty(sorting))
        {
            if (sorting.Contains("DESC"))
            {
                var sortingSplit = sorting.Split(' ').ToList();
                sorting = sortingSplit[0] + " descending";
            }
            query = query.OrderBy(sorting);
        }

        return query;
    }
    #endregion
}

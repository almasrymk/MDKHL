

using SmartParking.Entities.Organizations;
using SmartParking.Entities.Subscriptions;
using SmartParking.Enums;
using SmartParking.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using System.Linq.Dynamic.Core;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using SmartParking.Filters;

namespace SmartParking.Subscriptions;
[Authorize]
public class SubscriptionAppService : CrudAppService<Subscription,SubscriptionDto,int,PagedAndSortedResultRequestDto,CreateUpdateSubscriptionDto>, ISubscriptionAppService
{
    private readonly IRepository<Subscription, int> _subscriptionRepository;
    private IDataFilter _dataFilter;
    public SubscriptionAppService(IRepository<Subscription,int> subscriptionRepository,IDataFilter dataFilter)
    : base(subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
       _dataFilter = dataFilter;
    }
    #region public methods 

    /// <inheritdoc/>
    public override async Task<SubscriptionDto> CreateAsync(CreateUpdateSubscriptionDto input)
    {
        if (input.Status != SubscriptionStatus.Draft && input.Status != SubscriptionStatus.Inactive && input.Status != SubscriptionStatus.Active)
            throw new InvalidOperationException("Invalid status value.");
        else
            input.Status = SubscriptionStatus.Draft;
        if (CurrentUser.IsInRole("MdkhlAdmin"))
        {
            using (_dataFilter.Disable<IMultiTenant>())
            {
                return await base.CreateAsync(input);
            }
        }
        else
            return await base.CreateAsync(input);
    }

    /// <inheritdoc/>
    public override async Task<SubscriptionDto> UpdateAsync(int id, CreateUpdateSubscriptionDto input)
    {
        var existingSubscription = await _subscriptionRepository.GetAsync(id);

        if (input.Status != SubscriptionStatus.Draft && input.Status != SubscriptionStatus.Inactive && input.Status != SubscriptionStatus.Active)
            throw new InvalidOperationException("Invalid status value.");

        // If the current status is Draft, allow it to change to Active or Inactive
        if (existingSubscription.StatusSubscription == SubscriptionStatus.Draft)
        {
            if (input.Status == SubscriptionStatus.Active || input.Status == SubscriptionStatus.Inactive)
                existingSubscription.StatusSubscription = input.Status;


        }
        // If the current status is Active or Inactive, prevent it from changing back to Draft
        else if (existingSubscription.StatusSubscription == SubscriptionStatus.Active || existingSubscription.StatusSubscription == SubscriptionStatus.Inactive)
        {
            if (input.Status == SubscriptionStatus.Draft)
                throw new InvalidOperationException("Cannot change from Active/Inactive back to Draft.");
            else
                existingSubscription.StatusSubscription = input.Status;
        }

        if (existingSubscription.StatusSubscription != input.Status)
        {
            existingSubscription.StatusSubscription = input.Status;

            if (input.Status == SubscriptionStatus.Active)
                existingSubscription.ActivationDate = DateOnly.FromDateTime(DateTime.Now);
            else if (input.Status == SubscriptionStatus.Inactive)
                existingSubscription.InActivationDate = DateOnly.FromDateTime(DateTime.Now);
        }
        return await base.UpdateAsync(id, input);
    }

    /// <inheritdoc/>
    public override async Task<SubscriptionDto> GetAsync(int id)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            return await base.GetAsync(id);
        }
    }
    /// <inheritdoc/>
    public async Task<PagedResultDto<SubscriptionDto>> GetAllListAsync(PagedAndSortedAndSearchResultRequestDto input, Guid? tenantId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var queryable = await _subscriptionRepository.GetQueryableAsync();
            queryable = ApplyFilterTenant(tenantId, queryable);
            queryable = ApplySearch(input, queryable);
            queryable = ApplySort(input, queryable);
            var totalCount = await AsyncExecuter.CountAsync(queryable);
            List<Subscription> subscriptions = await ApplyPaged(input, queryable);
            var subscriptionsDto = ObjectMapper.Map<List<Subscription>, List<SubscriptionDto>>(subscriptions);
            return new PagedResultDto<SubscriptionDto>(
                totalCount,
                subscriptionsDto
            );
        }
    }
  
    /// <inheritdoc/>
    public override async Task DeleteAsync(int id)
    {
        var subscription = await _subscriptionRepository.FindAsync(id);
        if (subscription is null)
            throw new EntityNotFoundException(typeof(Subscription), id);
        await _subscriptionRepository.HardDeleteAsync(subscription);
    }

    /// <inheritdoc/>
    public async Task<PagedResultDto<SubscriptionDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input)
    {
        // Get the IQueryable<subscription> from the repository
        var queryable = await _subscriptionRepository.GetQueryableAsync();
        // Apply filtering
        var filterExpression = BuildExpression<Subscription>.BuildFilterExpression(input.filters);
        if (filterExpression is not null)
            queryable = queryable.Where(filterExpression);
        queryable = ApplySort(input, queryable);
        // Get the total count of filtered records
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        // Apply pagination
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        // Execute the query and get the list of subscriptions entities
        var subscriptions = await AsyncExecuter.ToListAsync(queryable);

        // Map the subscriptions entities to subscriptionsDto objects
        var subscriptionsDto = ObjectMapper.Map<List<Subscription>, List<SubscriptionDto>>(subscriptions);
        // Return the paginated result
        return new PagedResultDto<SubscriptionDto>(
            totalCount,
            subscriptionsDto
        );
    }

    /// <inheritdoc/>
    public async Task<PagedResultDto<SubscriptionDto>> Search(PagedAndSortedAndSearchResultRequestDto input)
    {
        // Get the IQueryable<camera> from the repository
        var queryable = await _subscriptionRepository.GetQueryableAsync();
        // Apply search filtering
        queryable = ApplySearch(input, queryable);
        queryable = ApplySort(input, queryable);
        // Get the total count of filtered records
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        // Apply pagination
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        // Execute the query and get the list of hotel entities
        var subscriptions = await AsyncExecuter.ToListAsync(queryable);

        // Map the hotel entities to hotelDto objects
        var subscriptionsDto = ObjectMapper.Map<List<Subscription>, List<SubscriptionDto>>(subscriptions);
        Console.WriteLine(subscriptionsDto.Count);
        // Return the paginated result
        return new PagedResultDto<SubscriptionDto>(
            totalCount,
            subscriptionsDto
        );
    }
    [RemoteService(IsEnabled = false)]
    /// <inheritdoc/>

    public async Task<List<SubscriptionDto>> GetAllAsync()
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var subscriptions = await _subscriptionRepository.GetListAsync();
            var subscriptionsMap = ObjectMapper.Map<List<Subscription>, List<SubscriptionDto>>(subscriptions);
            return subscriptionsMap;
        }
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Applies a filter for tenant ID on the given subscription query.
    /// </summary>
    /// <param name="tenantId">The tenant ID to filter subscriptions by. If null, no filter is applied.</param>
    /// <param name="queryable">The IQueryable of Subscription entities to apply the filter on.</param>
    /// <returns>The filtered <see cref="IQueryable{Subscription}"/> based on the tenant ID.</returns>
    private static IQueryable<Subscription> ApplyFilterTenant(Guid? tenantId, IQueryable<Subscription> queryable)
    {
        if (tenantId.HasValue)
            queryable = queryable.Where(x => x.TenantId == tenantId);
        return queryable;
    }


    /// <summary>
    /// Applies sorting to the given subscription query based on the input sorting string.
    /// </summary>
    /// <param name="input">The DTO containing sorting information.</param>
    /// <param name="queryable">The IQueryable of Subscription entities to apply sorting to.</param>
    /// <returns>The sorted <see cref="IQueryable{Subscription}"/> based on the sorting criteria.</returns>
    private static IQueryable<Subscription> ApplySort(PagedAndSortedAndSearchResultRequestDto input, IQueryable<Subscription> queryable)
    {
        if (!input.Sorting.IsNullOrEmpty())
        {
            if (input.Sorting.Contains("DESC"))
            {
                var sorting = input.Sorting.Split(' ').ToList();
                queryable = queryable.OrderBy(sorting[0] + " descending");
            }
            else
                queryable = queryable.OrderBy(input.Sorting);
        }
        return queryable;
    }
    /// <summary>
    /// Applies search criteria to the given subscription query based on the input search term.
    /// </summary>
    /// <param name="input">The DTO containing the search term.</param>
    /// <param name="queryable">The IQueryable of Subscription entities to apply search filters to.</param>
    /// <returns>The filtered <see cref="IQueryable{Subscription}"/> based on the search term.</returns>
    private static IQueryable<Subscription> ApplySearch(PagedAndSortedAndSearchResultRequestDto input, IQueryable<Subscription> queryable)
    {
        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {

            queryable = queryable.Where(or =>
                or.ArabicName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.SubscriptionPrice.ToString().ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.EnglishName.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.SubscriptionType.TypeAr.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                or.SubscriptionType.TypeEn.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                 or.SubscriptionDescription.ToLower().Contains(input.SearchingTerm.ToLower()) ||
                 or.StatusSubscription.ToString().ToLower().Contains(input.SearchingTerm.ToLower())
            );
        }

        return queryable;
    }
    /// <summary>
    /// Applies paging to the provided subscription queryable based on the given input parameters.
    /// </summary>
    /// <param name="input">The DTO containing the skip count and maximum result count for paging.</param>
    /// <param name="queryable">The IQueryable of Subscription entities to apply the paging to.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a list of <see cref="Subscription"/> entities.</returns>
    private async Task<List<Subscription>> ApplyPaged(PagedAndSortedAndSearchResultRequestDto input, IQueryable<Subscription> queryable)
    {
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        var subscriptions = await AsyncExecuter.ToListAsync(queryable);
        return subscriptions;
    }
    #endregion
}

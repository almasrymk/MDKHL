

using SmartParking.Entities.Subscriptions;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Subscriptions;

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

    /// <inheritdoc/>
    public override async Task<SubscriptionDto> CreateAsync(CreateUpdateSubscriptionDto input)
    {
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

        // If the current status is Draft, allow it to change to Active or Inactive
        if (existingSubscription.Status == SubscriptionStatus.Draft)
        {
            if (input.Status == SubscriptionStatus.Active || input.Status == SubscriptionStatus.Inactive)
            {
                // Change status to the new value (Active or Inactive)
                existingSubscription.Status = input.Status;
            }
            
        }
        // If the current status is Active or Inactive, prevent it from changing back to Draft
        else if (existingSubscription.Status == SubscriptionStatus.Active || existingSubscription.Status == SubscriptionStatus.Inactive)
        {
            if (input.Status == SubscriptionStatus.Draft)
            {
                throw new InvalidOperationException("Cannot change from Active/Inactive back to Draft.");
            }
            else
            {
                // Allow other status changes
                existingSubscription.Status = input.Status;
            }
        }

        if (CurrentUser.IsInRole("MdkhlAdmin"))
        {
                using (_dataFilter.Disable<IMultiTenant>())
                {
                    return await base.UpdateAsync(id, input);
                }
        }
        else
             return await base.UpdateAsync(id, input);
        
    
    }

    /// <inheritdoc/>
    public override async Task<SubscriptionDto> GetAsync(int id)
    {
        if (CurrentUser.IsInRole("MdkhlAdmin"))
        {
            using (_dataFilter.Disable<IMultiTenant>())
            {
                return await base.GetAsync(id);
            }
        }
        else
            return await base.GetAsync(id);
    }
    /// <inheritdoc/>
    public override async Task<PagedResultDto<SubscriptionDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        if (CurrentUser.IsInRole("MdkhlAdmin"))
        {
            using (_dataFilter.Disable<IMultiTenant>())
            {
                return await base.GetListAsync(input);
            }
        }
        else
            return await base.GetListAsync(input);
    }

    /// <inheritdoc/>
    public override async Task DeleteAsync(int id)
    {
        var subscription =  await _subscriptionRepository.GetAsync(id);
        if (subscription is null)
        {
            throw new EntityNotFoundException(typeof(Subscription), id);
        }

        if (CurrentUser.IsInRole("MdkhlAdmin"))
        {
            using (_dataFilter.Disable<IMultiTenant>())
            {
                await base.DeleteAsync(id);
            }
        }
        else
             await base.DeleteAsync(id);
    }

    /// <inheritdoc/>
    public async Task<PagedResultDto<SubscriptionDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input)
    {
        // Get the IQueryable<subscription> from the repository
        var queryable = await _subscriptionRepository.GetQueryableAsync();
        // Apply filtering
        var filterExpression = BuildExpression<Subscription>.BuildFilterExpression(input.filters);
        if (filterExpression is not null)
        {
            queryable = queryable.Where(filterExpression);
        }
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
        if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
        {

            queryable = queryable.Where(sub =>
                sub.Status.ToString().StartsWith(input.SearchingTerm)
            );
        }
        // Get the total count of filtered records
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        // Apply pagination
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);

        // Execute the query and get the list of subscription entities

        var subscriptions = await AsyncExecuter.ToListAsync(queryable);

        // Map the hotel entities to subscriptionsDto objects
        var subscriptionsDto = ObjectMapper.Map<List<Subscription>, List<SubscriptionDto>>(subscriptions);
        // Return the paginated result
        return new PagedResultDto<SubscriptionDto>(
            totalCount,
            subscriptionsDto
        );
    }

}

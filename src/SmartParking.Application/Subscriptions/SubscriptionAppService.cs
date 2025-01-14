

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

    /// <inheritdoc/>
    public override async Task<SubscriptionDto> CreateAsync(CreateUpdateSubscriptionDto input)
    {
        if(input.Status != SubscriptionStatus.Draft && input.Status !=SubscriptionStatus.Inactive && input.Status != SubscriptionStatus.Active)
        {
            throw new InvalidOperationException("Invalid status value.");

        }
        else
        {
            input.Status = SubscriptionStatus.Draft;
        }


        

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
        {
            throw new InvalidOperationException("Invalid status value.");

        }
        
        // If the current status is Draft, allow it to change to Active or Inactive
        if (existingSubscription.StatusSubscription == SubscriptionStatus.Draft)
        {
            if (input.Status == SubscriptionStatus.Active || input.Status == SubscriptionStatus.Inactive)
            {
                // Change status to the new value (Active or Inactive)
                existingSubscription.StatusSubscription = input.Status;
            }
            
        }
        // If the current status is Active or Inactive, prevent it from changing back to Draft
        else if (existingSubscription.StatusSubscription == SubscriptionStatus.Active || existingSubscription.StatusSubscription == SubscriptionStatus.Inactive)
        {
            if (input.Status == SubscriptionStatus.Draft)
            {
                throw new InvalidOperationException("Cannot change from Active/Inactive back to Draft.");
            }
            else
            {
                // Allow other status changes
                existingSubscription.StatusSubscription = input.Status;
            }
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
    public override async Task<PagedResultDto<SubscriptionDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
       
            using (_dataFilter.Disable<IMultiTenant>())
            {
                return await base.GetListAsync(input);
            }
    }

    /// <inheritdoc/>
    public override async Task DeleteAsync(int id)
    {
        var subscription =  await _subscriptionRepository.FindAsync(id);

        if (subscription is null)
        {
            throw new EntityNotFoundException(typeof(Subscription), id);
        }


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
        {
            queryable = queryable.Where(filterExpression);
        }
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
    [RemoteService(IsEnabled =false)]
    public async Task<List<SubscriptionDto>> GetAllAsync()
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var subscriptions = await _subscriptionRepository.GetListAsync();
            var subscriptionsMap = ObjectMapper.Map<List<Subscription>, List<SubscriptionDto>>(subscriptions);
            return subscriptionsMap;
        }
            
    }
}

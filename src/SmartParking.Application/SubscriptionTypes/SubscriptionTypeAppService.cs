using Microsoft.AspNetCore.Authorization;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Subscriptions;
using SmartParking.Enums;
using SmartParking.Garages;
using SmartParking.Subscriptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.SubscriptionTypes;

[Authorize]
public class SubscriptionTypeAppService : CrudAppService<SubscriptionType, SubscriptionTypeDto, int, PagedAndSortedResultRequestDto, CreateUpdateSubscriptionTypeDto>, ISubscriptionTypeAppService
{
    private readonly IRepository<SubscriptionType, int> _subscriptionTypeRepository;
    private IDataFilter _dataFilter;

    public SubscriptionTypeAppService(IRepository<SubscriptionType, int> subscriptionTypeRepository,IDataFilter dataFilter)
    : base(subscriptionTypeRepository)
    {
        _subscriptionTypeRepository = subscriptionTypeRepository;
        _dataFilter = dataFilter;
    }
    /// <inheritdoc/>
    public override async Task<SubscriptionTypeDto> CreateAsync(CreateUpdateSubscriptionTypeDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var existingSubscriptionType = await _subscriptionTypeRepository.FirstOrDefaultAsync(x => x.TypeEn == input.TypeEn );

            if (existingSubscriptionType != null)
            {
                throw new InvalidOperationException("subscription type with the same name already exists !");
            }
                return await base.CreateAsync(input);
        }
    }

    /// <inheritdoc/>
    public override async Task<SubscriptionTypeDto> UpdateAsync(int id, CreateUpdateSubscriptionTypeDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {

            var existingSubscriptionType = await _subscriptionTypeRepository.FirstOrDefaultAsync(x => x.TypeEn == input.TypeEn);

            if (existingSubscriptionType != null)
            {
                throw new InvalidOperationException("subscription type with the same name already exists !");
            }

            return await base.UpdateAsync(id, input);
        }
    }

    ///<inheritdoc/>
    public override async Task DeleteAsync(int id)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {

            var subscription = await _subscriptionTypeRepository.FindAsync(id);
            if (subscription is null)
            {
                throw new EntityNotFoundException(typeof(SubscriptionType), id);
            }
            // await base.DeleteAsync(id);

            subscription.Status = Status.Deleted;
            await CurrentUnitOfWork.SaveChangesAsync();
        }
    }

    /// <inheritdoc/>
    public async Task<List<SubscriptionTypeLookupDto>> GetSubscriptionTypeLookupAsync()
	{
        using (DataFilter.Disable<IMultiTenant>())
        {


		    var subscriptionTypes = await _subscriptionTypeRepository.GetListAsync();
		    var subscriptionTypesList = ObjectMapper.Map<List<SubscriptionType>, List<SubscriptionTypeLookupDto>>(subscriptionTypes)
    ;
		    return subscriptionTypesList;
        }
	}
	/// <inheritdoc/>
	public async Task<PagedResultDto<SubscriptionTypeDto>> FilterForMDKHL(PagedAndSortedAndFilteredResultRequestDto input)
	{
		using (_dataFilter.Disable<IMultiTenant>())
		{
			// Get the IQueryable<subscriptionTypes> from the repository
			var queryable = await _subscriptionTypeRepository.GetQueryableAsync();
			// Apply filtering
			var filterExpression = BuildExpression<SubscriptionType>.BuildFilterExpression(input.filters);
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
			// Execute the query and get the list of subscriptionTypes entities
			var subscriptionTypes = await AsyncExecuter.ToListAsync(queryable);

			// Map the subscriptionTypes entities to subscriptionTypesDto objects
			var subscriptionTypesDto = ObjectMapper.Map<List<SubscriptionType>, List<SubscriptionTypeDto>>(subscriptionTypes);
			// Return the paginated result
			return new PagedResultDto<SubscriptionTypeDto>(
				totalCount,
				subscriptionTypesDto
			);
		}

	}
	public async Task<PagedResultDto<SubscriptionTypeDto>> SearchForMDKHL(PagedAndSortedAndSearchResultRequestDto input)
	{
		using (_dataFilter.Disable<IMultiTenant>())
		{
			// Get the IQueryable<garage> from the repository
			var queryable = await _subscriptionTypeRepository.GetQueryableAsync();
			// Apply search filtering
			if (!string.IsNullOrWhiteSpace(input.SearchingTerm))
			{

				queryable = queryable.Where(or =>
					or.TypeEn.ToLower().Contains(input.SearchingTerm.ToLower()) ||
					or.TypeAr.ToLower().Contains(input.SearchingTerm.ToLower()) 

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
			var SubscriptionTypes = await AsyncExecuter.ToListAsync(queryable);

			// Map the hotel entities to hotelDto objects
			var SubscriptionTypesDto = ObjectMapper.Map<List<SubscriptionType>, List<SubscriptionTypeDto>>(SubscriptionTypes);
			Console.WriteLine(SubscriptionTypesDto.Count);
			// Return the paginated result
			return new PagedResultDto<SubscriptionTypeDto>(
				totalCount,
				SubscriptionTypesDto
			);
		}
	}
}

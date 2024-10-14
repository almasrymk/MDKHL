using SmartParking.Entities.Subscriptions;
using SmartParking.Subscriptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.SubscriptionTypes;

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
        var existingSubscriptionType = await _subscriptionTypeRepository.FirstOrDefaultAsync(x => x.Type == input.Type);

        if (existingSubscriptionType != null)
        {
            throw new InvalidOperationException("subscription type with the same name already exists !");
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
    public override async Task<SubscriptionTypeDto> UpdateAsync(int id, CreateUpdateSubscriptionTypeDto input)
    {
        var existingSubscriptionType = await _subscriptionTypeRepository.FirstOrDefaultAsync(x => x.Type == input.Type);

        if (existingSubscriptionType != null)
        {
            throw new InvalidOperationException("subscription type with the same name already exists !");
        }

        if (CurrentUser.IsInRole("MdkhlAdmin"))
        {
            using (_dataFilter.Disable<IMultiTenant>())
            {
                return await base.UpdateAsync(id, input);
            }
        }
        else
            return await base.UpdateAsync(id,input);
    }

    ///<inheritdoc/>
    public override async Task DeleteAsync(int id)
    {
        var subscription = await _subscriptionTypeRepository.GetAsync(id);
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
    public async Task<List<SubscriptionTypeLookupDto>> GetSubscriptionTypeLookupAsync()
	{
		var subscriptionTypes = await _subscriptionTypeRepository.GetListAsync();
		var subscriptionTypesList = ObjectMapper.Map<List<SubscriptionType>, List<SubscriptionTypeLookupDto>>(subscriptionTypes)
;
		return subscriptionTypesList;
	}
}

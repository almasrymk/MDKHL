using SmartParking.Entities;
using SmartParking.Entities.Subscriptions;
using SmartParking.Subscriptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.SubscriptionTypes;

public class SubscriptionTypeAppService : CrudAppService<SubscriptionType, SubscriptionTypeDto, int, PagedAndSortedResultRequestDto, CreateUpdateSubscriptionTypeDto>, ISubscriptionTypeAppService
{
    private readonly IRepository<SubscriptionType, int> _subscriptionTypeRepository;
    public SubscriptionTypeAppService(IRepository<SubscriptionType, int> subscriptionTypeRepository)
    : base(subscriptionTypeRepository)
    {
        _subscriptionTypeRepository = subscriptionTypeRepository;
    }

    public async Task<List<SubscriptionTypeDto>> GetSubscriptionTypeLookupAsync()
    {
        var subscriptionTypes = await _subscriptionTypeRepository.GetListAsync();
        var mappedSubscriptionTypes = ObjectMapper.Map<List<SubscriptionType>, List<SubscriptionTypeDto>>(subscriptionTypes);
        return mappedSubscriptionTypes;
    }
}

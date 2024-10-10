using SmartParking.Entities.Subscriptions;
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

	public async Task<List<SubscriptionTypeLookupDto>> GetSubscriptionTypeLookupAsync()
	{
		var subscriptionTypes = await _subscriptionTypeRepository.GetListAsync();
		var subscriptionTypesList = ObjectMapper.Map<List<SubscriptionType>, List<SubscriptionTypeLookupDto>>(subscriptionTypes)
;
		return subscriptionTypesList;
	}
}

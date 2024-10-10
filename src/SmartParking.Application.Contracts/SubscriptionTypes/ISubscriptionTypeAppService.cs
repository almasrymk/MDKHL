using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.SubscriptionTypes;

public interface ISubscriptionTypeAppService : ICrudAppService<SubscriptionTypeDto,int,PagedAndSortedResultRequestDto,CreateUpdateSubscriptionTypeDto>
{
	/// <summary>
	/// abstract method to get of subscriptionTypes
	/// </summary>
	/// <returns>SubscriptionTypeLookupDto</returns>
	Task<List<SubscriptionTypeLookupDto>> GetSubscriptionTypeLookupAsync();
}

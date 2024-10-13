using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.SubscriptionTypes;

public interface ISubscriptionTypeAppService : ICrudAppService<SubscriptionTypeDto,int,PagedAndSortedResultRequestDto,CreateUpdateSubscriptionTypeDto>
{

    /// <summary>
    /// abstract method to create subscription type
    /// </summary>
    /// <param name="input">subscription type create dto</param>
    /// <returns>SubscriptionTypeDto</returns>
    public Task<SubscriptionTypeDto> CreateAsync(CreateUpdateSubscriptionTypeDto input);

    /// <summary>
    /// abstract method to update specific subscription type 
    /// </summary>
    /// <param name="input">subscription type create dto</param>
    /// <param name="id">id of specific subscription type </param>
    /// <returns>SubscriptionDto</returns>
    public Task<SubscriptionTypeDto> UpdateAsync(int id, CreateUpdateSubscriptionTypeDto input);

    /// <summary>
    /// abstract method to delete specific subscription type 
    /// </summary>
    /// <param name="input">id of specific subscription type </param>

    public Task DeleteAsync(int id);
    /// <summary>
    /// abstract method to get of subscriptionTypes
    /// </summary>
    /// <returns>SubscriptionTypeLookupDto</returns>
    Task<List<SubscriptionTypeLookupDto>> GetSubscriptionTypeLookupAsync();

  
}

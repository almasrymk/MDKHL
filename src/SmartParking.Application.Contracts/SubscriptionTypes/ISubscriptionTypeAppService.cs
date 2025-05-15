using SmartParking.Garages;
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
    Task<SubscriptionTypeDto> CreateAsync(CreateUpdateSubscriptionTypeDto input);

    /// <summary>
    /// abstract method to update specific subscription type 
    /// </summary>
    /// <param name="input">subscription type create dto</param>
    /// <param name="id">id of specific subscription type </param>
    /// <returns>SubscriptionDto</returns>
    Task<SubscriptionTypeDto> UpdateAsync(int id, CreateUpdateSubscriptionTypeDto input);

    /// <summary>
    /// abstract method to delete specific subscription type 
    /// </summary>
    /// <param name="input">id of specific subscription type </param>

    Task DeleteAsync(int id);
    /// <summary>
    /// abstract method to get of subscriptionTypes
    /// </summary>
    /// <returns>SubscriptionTypeLookupDto</returns>
    Task<List<SubscriptionTypeLookupDto>> GetSubscriptionTypeLookupAsync();
    /// <summary>
    /// Method used to filter column. 
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<PagedResultDto<SubscriptionTypeDto>> FilterForMDKHL(PagedAndSortedAndFilteredResultRequestDto input);
    /// <summary>
    /// Method used to search . 
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<PagedResultDto<SubscriptionTypeDto>> SearchForMDKHL(PagedAndSortedAndSearchResultRequestDto input);



}

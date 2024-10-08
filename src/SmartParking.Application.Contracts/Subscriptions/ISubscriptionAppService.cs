using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Subscriptions;

public interface ISubscriptionAppService : ICrudAppService<SubscriptionDto,int, PagedAndSortedResultRequestDto, CreateUpdateSubscriptionDto>
{
    /// <summary>
    /// abstract method to create subscription
    /// </summary>
    /// <param name="input">subscription create dto</param>
    /// <returns>SubscriptionDto</returns>
    public Task<SubscriptionDto> CreateAsync(CreateUpdateSubscriptionDto input);

    /// <summary>
    /// abstract method to update specific subscription
    /// </summary>
    /// <param name="input">subscription create dto</param>
    /// <returns>SubscriptionDto</returns>
    public Task<SubscriptionDto> UpdateAsync(int id,CreateUpdateSubscriptionDto input);

    /// <summary>
    /// abstract method to get specific subscription
    /// </summary>
    /// <param name="input">id of specific subscription</param>
    /// <returns>SubscriptionDto</returns>
    public Task<SubscriptionDto> GetAsync(int id);

    /// <summary>
    /// abstract method to get subscriptions
    /// </summary>
    /// <param name="input"> PagedAndSortedResultRequestDto</param>
    /// <returns>PagedResultDto of SubscriptionDto</returns>
    public Task<PagedResultDto<SubscriptionDto>> GetListAsync(PagedAndSortedResultRequestDto input);

    /// <summary>
    /// abstract method to delete specific subscription
    /// </summary>
    /// <param name="input">id of specific subscription</param>
   
    public Task DeleteAsync(int id);

    /// <summary>
    /// abstract method to search of subscriptions
    /// </summary>
    /// <param name="input">specifies max number of subscription and if it sorted or not and search term</param>
    /// <returns>SubscriptionDto</returns>
    public Task<PagedResultDto<SubscriptionDto>> Search(PagedAndSortedAndSearchResultRequestDto input);

    /// <summary>
    /// abstract method to filter of subscriptions
    /// </summary>
    /// <param name="input">specifies max number of subscriptions and if it sorted or not and filter</param>
    /// <returns>SubscriptionDto</returns>
    public Task<PagedResultDto<SubscriptionDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);
}

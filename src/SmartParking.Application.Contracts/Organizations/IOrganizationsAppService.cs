using SmartParking.Results;
using SmartParking.Subscriptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Organizations;

public interface IOrganizationsAppService : ICrudAppService<OrganizationDto,int,PagedAndSortedResultRequestDto,CreateUpdateOrganiztionDto>
{
    /// <summary>
    /// abstract method to create organization
    /// </summary>
    /// <param name="input">Organization create dto</param>
    /// <returns>Organization Dto</returns>
    Task<OrganizationDto> CreateAsync(CreateUpdateOrganiztionDto input);


    /// <summary>
    /// abstract method to get list of organizations
    /// </summary>
    /// <param name="input">specifies max number of organization and if it sorted or not</param>
    /// <returns>Organization Dto</returns>
    Task<PagedResultDto<OrganizationDto>> GetListAsync(PagedAndSortedResultRequestDto input);

    /// <summary>
    /// abstract method to get organization
    /// </summary>
    /// <param name="input">id of organization</param>
    /// <returns>Organization Dto</returns>
    Task<OrganizationDto> GetAsync(int id);

    /// <summary>
    /// abstract method used to update organization
    /// </summary>
    /// <param name="id">organization id</param>
    /// <param name="input">dto of create organization</param>
    /// <returns>organization dto of update one</returns>
    Task<OrganizationDto> UpdateAsync(int id, CreateUpdateOrganiztionDto input);

    /// <summary>
    /// abstract method to delete organization
    /// </summary>
    /// <param name="id">id of organization</param>
    /// <returns>successfull message of deletion</returns>
    Task<string> DeleteByIdAsync(int id);

    /// <summary>
    /// abstract method to search of organizations
    /// </summary>
    /// <param name="input">specifies max number of organization and if it sorted or not and search term</param>
    /// <returns>Organization Dto</returns>
    public Task<PagedResultDto<OrganizationDto>> Search(PagedAndSortedAndSearchResultRequestDto input);

    /// <summary>
    /// abstract method to filter of organizations
    /// </summary>
    /// <param name="input">specifies max number of organization and if it sorted or not and filter</param>
    /// <returns>Organization Dto</returns>
    public Task<PagedResultDto<OrganizationDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);
    /// <summary>
    /// Retrieves a list of all organizations asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing a list of <see cref="OrganizationDto"/> objects.</returns>
    public Task<Result<List<OrganizationDto>>> GetAllMobileAsync();
    /// <summary>
    /// Method adds specific subscription for an organization.
    /// </summary>
    /// <param name="OrganizationId">OrganizationId</param>
    /// <param name="SubscriptionId">SubscriptionId</param>
    /// <returns>SubscriptionDto</returns>
    Task<SubscriptionDto> AddOrganizationForSubscription(int OrganizationId , int SubscriptionId);

    /// <summary>
    /// Method used to get Count of Organizations.
    /// </summary>
    /// <returns>Count of Organizations</returns>
    Task<int> GetOrganizationsCount();

}

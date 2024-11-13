using SmartParking.Authentication;
using SmartParking.Entities.Organizations;
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
    public  Task<OrganizationDto> CreateAsync(CreateUpdateOrganiztionDto input);


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
    /// abstract method to get organization id
    /// </summary>
    /// <param name="tenantId">id of tenant</param>
    /// <returns>Organization id</returns>
    Task<int> GetOrganizationIdByTenant(Guid? tenantId);

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

    /// <summary>
    /// Method that returns Organization for current tenant
    /// </summary>
    /// <returns>Organziation entity</returns>
    Task<Organization> GetOrganizationForCurrentTenat();

    /// <summary>
    /// Method used to get maximum number of Garages organization can add depends on organization subscription. 
    /// </summary>
    /// <returns>Maximum number of garages organization can add</returns>
    Task<int> GetNumberOfGaragesAllowedToBeAdded();

    /// <summary>
    /// Method used to get number of active garages belongs to organization.
    /// </summary>
    /// <returns>Number of active garages</returns>
    Task<int> GetNumberOfActiveGaragesOfOrganization();
    /// <summary>
    /// method used to retrieve organization image by its id.
    /// </summary>
    /// <param name="OrganizationId">OrganizationId</param>
    /// <returns>StringOfBase64forImage</returns>
    Task<string> GetOrganizationPictureByOrganizationTenant(Guid? OrganizationTenantId);

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.CustomerCategories;

public interface ICustomerCategoryAppService : ICrudAppService<CustomerCategoryDto,int,PagedAndSortedResultRequestDto,CreateUpdateCutomerCategoryDto>
{
    /// <summary>
    /// abstract method to search of customer
    /// </summary>
    /// <param name="input">specifies max number of organization and if it sorted or not and search term</param>
    /// <returns>Organization Dto</returns>
    public Task<PagedResultDto<CustomerCategoryDto>> Search(PagedAndSortedAndSearchResultRequestDto input);

    /// <summary>
    /// abstract method to filter of customer
    /// </summary>
    /// <param name="input">specifies max number of organization and if it sorted or not and filter</param>
    /// <returns>Organization Dto</returns>
    public Task<PagedResultDto<CustomerCategoryDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);
    /// <summary>
    /// Retrieves the customer categories associated with a specific organization.
    /// </summary>
    /// <param name="OrganizationId">The ID of the organization to filter the customer categories by.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="PagedResultDto{CustomerCategoryDto}"/> 
    /// containing the customer categories for the specified organization.</returns>
    public Task<PagedResultDto<CustomerCategoryDto>> GetCustomerCategoriesByOrganizationId(int OrganizationId);

    /// <summary>
    /// Method used to delete exists customer category
    /// </summary>
    /// <param name="id">Id of customer category that needed to be deleted</param>
    /// <returns>No Content</returns>
    Task DeleteAsync(int id);
}

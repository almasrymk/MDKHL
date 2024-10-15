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

    public Task<PagedResultDto<CustomerCategoryDto>> GetCustomerCategoriesByOrganizationId(int OrganizationId);
}

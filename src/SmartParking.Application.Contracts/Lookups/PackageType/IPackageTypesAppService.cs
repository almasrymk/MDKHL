using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Lookups;

public interface IPackageTypesAppService : ICrudAppService<PackageTypeDto , int , PagedAndSortedResultRequestDto , CreateUpdatePackageTypeDto>
{


    /// <summary>
    /// abstract method to create package type 
    /// </summary>
    /// <param name="input">package type  create dto</param>
    /// <returns>package type Dto</returns>
     Task<PackageTypeDto> CreateAsync(CreateUpdatePackageTypeDto input);

    /// <summary>
    /// abstract method to update specific package type 
    /// </summary>
    /// <param name="input">package type  create dto</param>
    /// <param name="id">id of specific package type </param>
    /// <returns>package type Dto</returns>
     Task<PackageTypeDto> UpdateAsync(int id, CreateUpdatePackageTypeDto input);

    /// <summary>
    /// abstract method to search of package Types
    /// </summary>
    /// <param name="input">specifies max number of Package Type and if it sorted or not and search term</param>
    /// <returns>PackageTypeDto</returns>
     Task<PagedResultDto<PackageTypeDto>> Search(PagedAndSortedAndSearchResultRequestDto input);


    /// <summary>
    /// abstract method to filter of package Types
    /// </summary>
    /// <param name="input">specifies max number of Package Type and if it sorted or not and filter</param>
    /// <returns>PackageTypeDto</returns>
     Task<PagedResultDto<PackageTypeDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);

    /// <summary>
    /// Method used to delete exists package type
    /// </summary>
    /// <param name="id">Id of package type that needed to be deleted</param>
    /// <returns>No Content</returns>
     Task DeleteAsync(int id);
     Task<PagedResultDto<PackageTypeDto>> GetListAsync(PagedAndSortedResultRequestDto input);
     Task<List<PackageTypeDto>> GetAllListAsync();
}
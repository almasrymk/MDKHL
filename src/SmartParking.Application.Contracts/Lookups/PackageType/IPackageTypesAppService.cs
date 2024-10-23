using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Lookups;

public interface IPackageTypesAppService : ICrudAppService<PackageTypeDto , int , PagedAndSortedResultRequestDto , CreateUpdatePackageTypeDto>
{
    /// <summary>
    /// abstract method to search of package Types
    /// </summary>
    /// <param name="input">specifies max number of Package Type and if it sorted or not and search term</param>
    /// <returns>PackageTypeDto</returns>
    public Task<PagedResultDto<PackageTypeDto>> Search(PagedAndSortedAndSearchResultRequestDto input);


    /// <summary>
    /// abstract method to filter of package Types
    /// </summary>
    /// <param name="input">specifies max number of Package Type and if it sorted or not and filter</param>
    /// <returns>PackageTypeDto</returns>
    public Task<PagedResultDto<PackageTypeDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);
    public Task DeleteAsync(int id);
}

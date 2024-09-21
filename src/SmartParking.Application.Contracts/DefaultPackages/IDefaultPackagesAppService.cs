using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.DefaultPackages;

public interface IDefaultPackagesAppService : ICrudAppService
                                        <DefaultPackageDto,
                                         int,
                                         PagedAndSortedResultRequestDto,
                                         CreateUpdateDefaultPackageDto>
{
    /// <summary>
    /// abstract method to create DefaultPackage
    /// </summary>
    /// <param name="input">Create DefaultPackage dto</param>
    /// <returns>DefaultPackage Dto</returns>
    Task<DefaultPackageDto> CreateAsync(CreateUpdateDefaultPackageDto input);

    /// <summary>
    /// abstract method to get list of DefaultPackages
    /// </summary>
    /// <param name="input">specifies max number of DefaultPackages and if it sorted or not</param>
    /// <returns>DefaultPackage Dto</returns>
    Task<PagedResultDto<DefaultPackageDto>> GetListAsync(PagedAndSortedResultRequestDto input);

    /// <summary>
    /// abstract method to get DefaultPackage by id
    /// </summary>
    /// <param name="id">id of DefaultPackage</param>
    /// <returns>DefaultPackage Dto</returns>
    Task<DefaultPackageDto> GetAsync(int id);

    /// <summary>
    /// abstract method used to update DefaultPackage
    /// </summary>
    /// <param name="id">DefaultPackage id</param>
    /// <param name="input">updated DefaultPackage dto</param>
    /// <returns>updated DefaultPackage dto</returns>
    Task<DefaultPackageDto> UpdateAsync(int id, CreateUpdateDefaultPackageDto input);

    /// <summary>
    /// abstract method to delete DefaultPackage
    /// </summary>
    /// <param name="id">id of DefaultPackage</param>
    /// <returns>successfull message of deletion</returns>
    Task<string> DeleteByIdAsync(int id);

}

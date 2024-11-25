
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Organizations;

public interface IContactInfoAppService : ICrudAppService<ContactInfoDto, int, PagedAndSortedResultRequestDto, ContactInfoCreateUpdateDto>
{

    /// <summary>
    /// Method used to get contactInfos by organization
    /// </summary>
    /// <param name="organizationId">Id of organization that needed </param>
    /// <param name="input">specifies max number of contactInfos</param>
    /// <returns>No Content</returns>
    public Task<PagedResultDto<ContactInfoDto>> GetContactInfoByOrganization(PagedAndSortedResultRequestDto input, int organizationId);

    /// <summary>
    /// Method used to delete exists contact info
    /// </summary>
    /// <param name="id">Id of contact info that needed to be deleted</param>
    /// <returns>No Content</returns>
    Task DeleteAsync(int id);

    /// <summary>
    /// abstract method to search of contactInfos
    /// </summary>
    /// <param name="input">specifies max number of contactInfo and if it sorted or not and search term</param>
    /// <returns>ContactInfoDto</returns>
    Task<PagedResultDto<ContactInfoDto>> Search(PagedAndSortedAndSearchResultRequestDto input);

    /// <summary>
    /// abstract method to filter of contactInfos
    /// </summary>
    /// <param name="input">specifies max number of contactInfos and if it sorted or not and filter</param>
    /// <returns>ContactInfoDto</returns>
    Task<PagedResultDto<ContactInfoDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);
}

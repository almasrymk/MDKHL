using SmartParking.Cutomers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Organizations;

public interface IContactInfoAppService : ICrudAppService<ContactInfoDto, int, PagedAndSortedResultRequestDto, ContactInfoCreateUpdateDto>
{
    public Task<PagedResultDto<ContactInfoDto>> GetContactInfoByOrganization(PagedAndSortedResultRequestDto input, int organizationId);


    /// <summary>
    /// Method used to delete exists contact info
    /// </summary>
    /// <param name="id">Id of contact info that needed to be deleted</param>
    /// <returns>No Content</returns>
    public Task DeleteAsync(int id);

}

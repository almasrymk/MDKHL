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
    public Task DeleteAsync(int id);
}

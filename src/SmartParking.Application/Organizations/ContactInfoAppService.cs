
using SmartParking.Cutomers;
using SmartParking.Entities.Customers;
using SmartParking.Entities.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Organizations;

public class ContactInfoAppService : CrudAppService<ContactInfo, ContactInfoDto, int, PagedAndSortedResultRequestDto, ContactInfoCreateUpdateDto>, IContactInfoAppService
{
    private readonly IRepository<ContactInfo, int> _repository;

    public ContactInfoAppService(IRepository<ContactInfo, int> repository) : base(repository)
    {
        _repository = repository;
    }

    public async Task<PagedResultDto<ContactInfoDto>> GetContactInfoByOrganization(PagedAndSortedResultRequestDto input, int organizationId)
    {

        var query = await _repository.GetQueryableAsync();
        query = query.Where(x => x.OrganizationId == organizationId);
        var totalCount = await AsyncExecuter.CountAsync(query);
        query = query.Skip(input.SkipCount).Take(input.MaxResultCount);
        query.OrderBy(x => x.Id);
        var contacts = await AsyncExecuter.ToListAsync(query);
        var contactDto = ObjectMapper.Map<List<ContactInfo>, List<ContactInfoDto>>(contacts);
        return new PagedResultDto<ContactInfoDto>(
           totalCount,
           contactDto
       );

    }
}

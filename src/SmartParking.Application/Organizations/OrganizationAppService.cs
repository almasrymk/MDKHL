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

public class OrganizationAppService : CrudAppService<Organization, OrganizationDto, int, PagedAndSortedResultRequestDto, CreateUpdateOrganiztionDto>, IOrganizationAppService
{
    public OrganizationAppService(IRepository<Organization, int> repository) : base(repository)
    {
    }
}

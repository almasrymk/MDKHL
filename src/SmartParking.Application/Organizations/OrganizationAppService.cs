using SmartParking.CustomerCategories;
using SmartParking.Entities.Organizations;
using SmartParking.Validations.Organizations;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Organizations;

public class OrganizationAppService : CrudAppService<Organization, OrganizationDto, int, PagedAndSortedResultRequestDto, CreateUpdateOrganiztionDto>, IOrganizationAppService
{
    private readonly IRepository<Organization, int> _repository;
    private readonly ICustomerCategoryAppService _customerCategoryAppService;

    public OrganizationAppService(IRepository<Organization, int> repository , ICustomerCategoryAppService customerCategoryAppService) : base(repository)
    {
        _repository = repository;
  
        _customerCategoryAppService = customerCategoryAppService;
    }

    public override async Task<OrganizationDto> CreateAsync(CreateUpdateOrganiztionDto input)
    {

        var organization = ObjectMapper.Map<CreateUpdateOrganiztionDto,Organization>(input);
        _repository.EnableTracking();
        await _repository.InsertAsync(organization,autoSave:true);
        organization.Code = CodeGenerator(organization.Id);
        _repository.DisableTracking();
        return  ObjectMapper.Map<Organization, OrganizationDto>(organization);
        organization.Code = CodeGenerator(id);
        await _repository.InsertAsync(organization , autoSave:true);
        await _customerCategoryAppService.CreateAsync(new CreateUpdateCutomerCategoryDto { Name = "عام", Description = "جميع العملاء", Default = true, OrganizationId = id });
        return ObjectMapper.Map<Organization, OrganizationDto>(organization);

    }
    private string CodeGenerator(int id)
    {
        var idLen =(int) Math.Floor(Math.Log10(id) + 1);
        int zeros = (6 - idLen);
        string Code = "O";
        Code = Code.PadRight(zeros + 1, '0');
        Code += (id);
        return Code;
    }
}
 
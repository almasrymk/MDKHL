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

        int id =await _repository.CountAsync()+1;
        
        var organization = ObjectMapper.Map<CreateUpdateOrganiztionDto,Organization>(input);
        organization.Code = CodeGenerator(id);
        await _repository.InsertAsync(organization , autoSave:true);
        
        if (id == 1)
        {
            await _customerCategoryAppService.CreateAsync(new CreateUpdateCutomerCategoryDto { Name = "عام", Description = "جميع العملاء", Default = true, OrganizationId = id });
        }
        return  ObjectMapper.Map<Organization, OrganizationDto>(organization);

    }
    private string CodeGenerator(int id)
    {
        //7
        var idLen = Math.Log10(id) + 1;
        //-1
        int zeros = (int)(6.0 - idLen);
        //O999999
        string Code = "O";
        Code = Code.PadRight(zeros + 1, '0');
        Code += (id);
        return Code;
    }
}

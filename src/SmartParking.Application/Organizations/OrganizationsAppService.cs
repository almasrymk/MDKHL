using SmartParking.CustomerCategories;
using SmartParking.Entities.Organizations;
using SmartParking.EnumDescriptions;
using SmartParking.Enums;
using SmartParking.Validations.Organizations;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Organizations;

public class OrganizationsAppService : CrudAppService<Organization, OrganizationDto, int, PagedAndSortedResultRequestDto, CreateUpdateOrganiztionDto>, IOrganizationsAppService
{
    private readonly IRepository<Organization, int> _repository;
    private readonly ICustomerCategoryAppService _customerCategoryAppService;

    public IDataFilter<ISoftDelete> _softDeleteFilter { get; }

    public OrganizationsAppService(IRepository<Organization, int> repository , ICustomerCategoryAppService customerCategoryAppService, IDataFilter<ISoftDelete> softDeleteFilter) : base(repository)
    {
        _repository = repository;
  
        _customerCategoryAppService = customerCategoryAppService;
         _softDeleteFilter = softDeleteFilter;
    }

    public override async Task<OrganizationDto> CreateAsync(CreateUpdateOrganiztionDto input)
    {
        var query = await _repository.GetListAsync(x => x.UnifiedLicenseNum == input.UnifiedLicenseNum);
        if (query.Count>0)
        {
            throw new BusinessException(EnumValues.GetEnumDescription(OrganizationCreationValidations.UnifiedLicenseNumUnifiedLicenseNum) + query.First().ArabicName);

        }
        var organization = ObjectMapper.Map<CreateUpdateOrganiztionDto,Organization>(input);
        await _repository.InsertAsync(organization,autoSave:true);
        organization.Code = CodeGenerator(organization.Id);
        await _customerCategoryAppService.CreateAsync(new CreateUpdateCutomerCategoryDto { Name =EnumValues.GetEnumDescription(CustomerCategoriesDefaults.Public), Description=EnumValues.GetEnumDescription(CustomerCategoriesDefaults.AllCustomers), Default = true, OrganizationId = organization.Id , CreatedByOrganization=true});
        return ObjectMapper.Map<Organization, OrganizationDto>(organization);


    }

    public override async Task<PagedResultDto<OrganizationDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        using (_softDeleteFilter.Disable())
        {
            return await base.GetListAsync(input);
        }
       
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


    public new async Task<string> DeleteByIdAsync(int id)
    {
        var orgaization = await _repository.FindAsync(id);
        if(orgaization is null)
            throw new EntityNotFoundException(typeof(Organization),id);
       else
        {
            await _repository.DeleteAsync(id);
            orgaization.Status = Enums.Status.Deleted;
            
            return EnumValues.GetEnumDescription(OrganizationCreationValidations.OrganizationDeleteion); 
        }
    }
}
 
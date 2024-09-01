using SmartParking.Entities.Organizations;
using SmartParking.EnumDescriptions;
using SmartParking.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.CustomerCategories;

public class CustomerCategoryAppService : CrudAppService<CustomerCategory, CustomerCategoryDto, int, PagedAndSortedResultRequestDto, CreateUpdateCutomerCategoryDto>, ICustomerCategoryAppService
{
    private readonly IRepository<CustomerCategory, int> _repository;

    public CustomerCategoryAppService(IRepository<CustomerCategory, int> repository) : base(repository)
    {
        _repository = repository;
    }

    public override async Task<CustomerCategoryDto> CreateAsync(CreateUpdateCutomerCategoryDto input)
    {
        if (input.CreatedByOrganization)
        {
            return await base.CreateAsync(input);
        }
        var query = await  _repository.GetListAsync(x => x.OrganizationId == input.OrganizationId);
        if(query.Count == 0)
        {
            throw new EntityNotFoundException(typeof(Organization), input.OrganizationId);
        }
        else if(input.Default == true)
        {
            if(query.Count > 0)
            {
                var customerCategory = query.First(x => x.Default == true);
                customerCategory.Default = false;

            }

        }
        return await base.CreateAsync(input);
    }
    public override async Task<CustomerCategoryDto> UpdateAsync(int id, CreateUpdateCutomerCategoryDto input)
    {
        //object has been set to be Default is true (true in database) , and in update operation is set Default to be false
        var customerCategoryById = await _repository.FindAsync(x => x.Id == id);
        if(input.Default==false && customerCategoryById.Default == true)
        {
            throw new BusinessException(EnumValues.GetEnumDescription(CustomerCategoryValidations.DefaultIsTrueAndUpdatedToBeFalse) + " " + customerCategoryById.Organization.EnglishName);
        }


        if (input.Default==true) {
            var query = await _repository.GetListAsync(x => x.OrganizationId == input.OrganizationId);
            var customerCategory = query.FirstOrDefault(x => x.Default == true);
            customerCategory.Default = false;
        }
        
        return await base.UpdateAsync(id, input);
    }

    public async Task<PagedResultDto<CustomerCategoryDto>> GetCustomerCategoriesByOrganizationId(int OrganizationId)
    {

        var query =await _repository.GetListAsync(x => x.OrganizationId == OrganizationId);
        var list= ObjectMapper.Map<List<CustomerCategory>, List<CustomerCategoryDto>>(query);
        return new PagedResultDto<CustomerCategoryDto>(list.Count,list);

    }
    public override async Task DeleteAsync(int id)
    {
        var customerCategory=await _repository.FirstOrDefaultAsync(x => x.Id == id);
        if (customerCategory.Default == true) {

            throw new BusinessException(EnumValues.GetEnumDescription(CustomerCategoryValidations.DeleteDefaultForOrganization) +" " +customerCategory.Organization.EnglishName );

        }

        await base.DeleteAsync(id);
    }
}

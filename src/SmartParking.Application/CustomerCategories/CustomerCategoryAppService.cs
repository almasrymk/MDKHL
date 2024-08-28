using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        var query = await  _repository.GetListAsync(x => x.OrganizationId == input.OrganizationId);
        if(query.Count ==0)
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
    public async Task<List<CustomerCategoryDto>> GetCustomerCategoriesByOrganizationId(int OrganizationId)
    {

        var query =await _repository.GetListAsync(x => x.OrganizationId == OrganizationId);
        return ObjectMapper.Map<List<CustomerCategory>, List<CustomerCategoryDto>>(query);

    }
}

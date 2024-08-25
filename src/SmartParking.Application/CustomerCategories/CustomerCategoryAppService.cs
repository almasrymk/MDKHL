using SmartParking.Entities.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.CustomerCategories;

public class CustomerCategoryAppService : CrudAppService<CustomerCategory, CustomerCategoryDto, int, PagedAndSortedResultRequestDto, CreateUpdateCutomerCategoryDto>, ICustomerCategoryAppService
{
    public CustomerCategoryAppService(IRepository<CustomerCategory, int> repository) : base(repository)
    {
    }
}

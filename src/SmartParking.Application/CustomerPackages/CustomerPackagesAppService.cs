using SmartParking.Cameras;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.CustomerPackages
{
    public class CustomerPackagesAppService:
         CrudAppService<
            CustomerPackage,
            CustomerPackageDto,
            int,
            PagedAndSortedResultRequestDto,
            CreateUpdateCustomerPackageDto
            >,
        ICustomerPackagesAppService
    {
        public CustomerPackagesAppService(IRepository<CustomerPackage, int> repository) :base(repository)
        {
            
        }
    }
}

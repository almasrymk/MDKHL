using AutoMapper.Internal.Mappers;
using SmartParking.Cameras;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Organizations;
using SmartParking.Garages;
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
        private readonly IRepository<CustomerPackage, int> _repository;

        public CustomerPackagesAppService(IRepository<CustomerPackage, int> repository) :base(repository)
        {
            _repository = repository;
        }
      
        public  CustomerPackageDto GetCustomerPackageByOrgIdAndCustomerId(int orgID,Guid customerID)
        {
            var result= _repository.FirstOrDefaultAsync(x=>x.Package.OrgnizationId == orgID&&x.CustomerId==customerID).Result;
            if (result == null)
            {
                return null; // Or throw an appropriate exception if necessary
            }
            return  ObjectMapper.Map<CustomerPackage,CustomerPackageDto>(result);
        }
    }
}

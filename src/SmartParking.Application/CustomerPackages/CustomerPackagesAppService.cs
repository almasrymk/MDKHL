using AutoMapper.Internal.Mappers;
using Microsoft.AspNetCore.Authorization;
using SmartParking.Cameras;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.Garages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.CustomerPackages;
[Authorize]
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
  
    public  async Task<CustomerPackageDto> GetOneActiveCustomerPackageByOrgIdAndCustomerId(int orgID,Guid customerID)
    {
        var result=await _repository.FirstOrDefaultAsync(x=>x.Package.OrgnizationId == orgID&&x.CustomerId==customerID&&x.CustomerPackageStatus==CustomerPackageStatus.Active);
        if (result == null)
        {
            return null; // Or throw an appropriate exception if necessary
        }
        return  ObjectMapper.Map<CustomerPackage,CustomerPackageDto>(result);
    }

    public override async Task DeleteAsync(int id)
    {
        var customerPackage = await _repository.FindAsync(id);

        if (customerPackage is null)
        {
            throw new EntityNotFoundException(typeof(VehicleModel), id);
        }


        await base.DeleteAsync(id);
    }
}

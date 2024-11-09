using AutoMapper.Internal.Mappers;
using Microsoft.AspNetCore.Authorization;
using SmartParking.Cameras;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.Garages;
using SmartParking.Packages;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;

namespace SmartParking.CustomerPackages;
//[Authorize]
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
    private readonly IRepository<Package> _packageRepository;

    public CustomerPackagesAppService(IRepository<CustomerPackage, int> repository,IRepository<Package> packageRepository) :base(repository)
    {
        _repository = repository;
        _packageRepository = packageRepository;
    }
  
    public  async Task<CustomerPackageDto> GetOneActiveCustomerPackageByOrgIdAndCustomerId(int orgID,Guid customerID)
    {
        var result=await _repository.FirstOrDefaultAsync(x=>x.Package.OrganizationId == orgID&&x.CustomerId==customerID&&x.CustomerPackageStatus==CustomerPackageStatus.Active);
        
        if (result == null)
        {
            return null; // Or throw an appropriate exception if necessary
        }
       
        return  ObjectMapper.Map<CustomerPackage,CustomerPackageDto>(result);
    }
    public  async Task<Result<List<PackageDto>>> GetPackageByCustomer()
    {
        var query = await _packageRepository.GetQueryableAsync();
        query = query.Where(x => x.CustomerPackages.Any(c => c.Customer.IdentityUserId == CurrentUser.Id));

        var result = await AsyncExecuter.ToListAsync(query);
        
        if (result.Count==0)
        {
            return Result<List<PackageDto>>.NotFound();
        }
       var packageMap = ObjectMapper.Map<List<Package>, List<PackageDto>>(result);
        return Result<List<PackageDto>>.Success(packageMap);
    }

    public override async Task DeleteAsync(int id)
    {
        var customerPackage = await _repository.FindAsync(id);

        if (customerPackage is null)
        {
            throw new EntityNotFoundException(typeof(VehicleModel), id);
        }
        customerPackage.Status = Status.Deleted;
        await CurrentUnitOfWork.SaveChangesAsync();

        //await base.DeleteAsync(id);
    }
}

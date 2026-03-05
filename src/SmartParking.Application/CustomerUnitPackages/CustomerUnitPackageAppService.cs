using SmartParking.Cutomers;
using SmartParking.Entities.CustomerUnitPackages;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.Results;
using SmartParking.UnitPackages;
using SmartParking.Validations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.CustomerUnitPackages;

public class CustomerUnitPackageAppService : ReadOnlyAppService<CustomerUnitPackage, CustomerUnitPackageDto, int>, 
    ICustomerUnitPackageAppService

{
    private readonly IRepository<CustomerUnitPackage, int> _repository;
    private readonly IRepository<UnitPackage> _packageRepository;
    private readonly IDataFilter _dataFilter;
    private readonly ICustomerAppService _customerAppService;

    public CustomerUnitPackageAppService(IRepository<CustomerUnitPackage, int> repository,
        IRepository<UnitPackage> packageRepository,
        IDataFilter dataFilter,
        ICustomerAppService customerAppService) : base(repository)
    {
        _repository = repository;
        _packageRepository = packageRepository;
        _dataFilter = dataFilter;

        _customerAppService = customerAppService;
    }

    public async Task DeleteAsync(int id)
    {
        var customerPackage = await _repository.FindAsync(id);

        if (customerPackage is null)
        {
            throw new EntityNotFoundException(typeof(CustomerPackage), id);
        }
        customerPackage.Status = Status.Deleted;
        await _repository.UpdateAsync(customerPackage, true);
    }

    

    public async Task<Result<List<CustomerUnitPackageDto>>> GetUnitPackageByCustomer(Guid customerId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _repository.GetQueryableAsync();
            query = query.Where(c => c.Customer.Id == customerId);

            var result = await AsyncExecuter.ToListAsync(query);

            var packageMap = ObjectMapper.Map<List<CustomerUnitPackage>, List<CustomerUnitPackageDto>>(result);
            return Result<List<CustomerUnitPackageDto>>.Success(packageMap);
        }
    }

    public async Task<Result<CustomerUnitPackageDto>> AssignPackageToCustomerAsync(int packageId, Guid customerId, CustomerUnitPackageAssignationDto customerUnitPackageAssignationDto)
    {
        var customer = await _customerAppService.GetAsync(customerId);
        if (customer == null)
            return Result.NotFound("Customer not found");

        var package = await GetPackageAsync(packageId);
        if (package == null)
            return Result.NotFound("Package not found");

        return await AssignPackage(customer, package , customerUnitPackageAssignationDto);
    }

    public override async Task<CustomerUnitPackageDto> GetAsync(int id)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            return await base.GetAsync(id);
        }
    }

    #region Private methods

    private async Task<UnitPackageDto> GetPackageAsync(int packageId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var package = await _packageRepository.FirstOrDefaultAsync(p => p.Id == packageId);
            return ObjectMapper.Map<UnitPackage, UnitPackageDto>(package);
        }
    }

    private async Task<Result<CustomerUnitPackageDto>> AssignPackage(CustomerDto customer, UnitPackageDto package , CustomerUnitPackageAssignationDto customerUnitPackageAssignationDto)
    {
        CustomerCategoryStatus category;

        Enum.TryParse(
            package.Package.CustomerCategory,
            true,
            out category
        );

        CreateUpdateCustomerUnitPackageDto customerUnitPackageDto = new CreateUpdateCustomerUnitPackageDto
        {
            ArabicName = package.Package.ArabicName,
            EnglishName = package.Package.EnglishName,
            ActivationDate = PackageAssignationLogic.PackageActivationTime(package.Package.ActivationDate,package.Package.PublishFrom, package.Package.PackageCategoryType == PackageCategoryTypes.CustomPackage),
            Status = package.Package.Status,
            DectivationDate = PackageAssignationLogic.PackageExpirationTime(package.Package.DectivationDate , package.Package.DurationOfPackage, package.Package.PackageCategoryType == PackageCategoryTypes.CustomPackage),
            Price = package.Package.Price,
            PublishFrom = package.Package.PublishFrom,
            PublishTo = package.Package.PublishTo,
            NumberOfSlots = package.NumberOfSlots,
            NumberOfVehicles = package.NumberOfVehicles,
            CustomerId = customer.Id,
            UnitPackageId = package.Id,
            DurationOfPackage=customerUnitPackageAssignationDto.DurationOfPackage,
            Vat=customerUnitPackageAssignationDto.Vat,
            OrganizationId=package.Package.OrganizationId,
            Discount=customerUnitPackageAssignationDto.Discount,
            CustomerCategory = category,
            UnitCode = customerUnitPackageAssignationDto.UnitCode
        };

        var entity = ObjectMapper.Map<CreateUpdateCustomerUnitPackageDto, CustomerUnitPackage>(customerUnitPackageDto);
        var data = await _repository.InsertAsync(entity);
        var dto = ObjectMapper.Map<CustomerUnitPackage, CustomerUnitPackageDto>(data);
        return dto;
    }

    public Task<bool> IfCustomerHasMultiplePackageAsync(Guid CustomerId)
    {
        return _repository.AnyAsync(x=> x.CustomerId == CustomerId);
    }

    public async Task<bool> CheckCustomerUnitPackageVehicleLimitations(Guid CustomerId , int numberOfVehicles)
    {
      var customerUnitPackge = await _repository.SingleOrDefaultAsync(x=>x.CustomerId == CustomerId);
        if (customerUnitPackge == null) {
            throw new Exception("NotFound");
            
        }
        return customerUnitPackge!.NumberOfVehicles > numberOfVehicles;
    }


    #endregion

}

using SmartParking.Cutomers;
using SmartParking.Entities.CustomerUnitPackages;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Units;
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
    private readonly IRepository<Unit> _unitRepository;

    public CustomerUnitPackageAppService(IRepository<CustomerUnitPackage, int> repository,
        IRepository<UnitPackage> packageRepository,
        IRepository<Unit> unitRepository,
        IDataFilter dataFilter,
        ICustomerAppService customerAppService) : base(repository)
    {
        _repository = repository;
        _packageRepository = packageRepository;
        _dataFilter = dataFilter;
        _unitRepository = unitRepository;
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

    

    public async Task<Result<List<CustomerUnitPackageDto>>> GetListOfUnitPackageByCustomer(Guid customerId , int organizationId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _repository.GetQueryableAsync();
            query = query
                .Where(c => c.Customer.Id == customerId && c.OrganizationId==organizationId)
                .OrderBy(c => Math.Abs((c.ActivationDate - DateTime.UtcNow).TotalSeconds));
            


            var result = await AsyncExecuter.ToListAsync(query);

            var packageMap = ObjectMapper.Map<List<CustomerUnitPackage>, List<CustomerUnitPackageDto>>(result);
            return Result<List<CustomerUnitPackageDto>>.Success(packageMap);
        
        }
        
    }

    public async Task<Result<CustomerUnitPackageDto>> AssignPackageToCustomerAsync(UnitPackageDto packagedto, Guid customerId, CustomerUnitPackageAssignationDto customerUnitPackageAssignationDto)
    {
        var customer = await _customerAppService.GetAsync(customerId);
        if (customer == null)
            return Result.NotFound("Customer not found");

        return await AssignPackage(customer, packagedto, customerUnitPackageAssignationDto);
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

        var customerUnitPackageDto = CreateUpdateCustomerUnitPackageDto.CreateFrom(customer, package, customerUnitPackageAssignationDto);

        var entity = ObjectMapper.Map<CreateUpdateCustomerUnitPackageDto, CustomerUnitPackage>(customerUnitPackageDto);
        var data = await _repository.InsertAsync(entity, autoSave: true);
        var dto = ObjectMapper.Map<CustomerUnitPackage, CustomerUnitPackageDto>(data);
        return dto;
    }

    public async Task<bool> IfCustomerHasMultiplePackageAsync(Guid CustomerId, UnitPackageDto dto)
    {
        return await _repository.AnyAsync(x => x.CustomerId == CustomerId
            && x.ActivationDate <= dto.Package.DectivationDate
            && x.DectivationDate >= dto.Package.ActivationDate);
    }
    public async Task<bool> CheckRenterSlotsOverUnit(UnitPackageDto dto, string unitCode)
    {

        var unit = await _unitRepository.FirstOrDefaultAsync(x => x.Code == unitCode);

        var count = await _repository.CountAsync(x => x.ActivationDate <= dto.Package.DectivationDate
            && x.DectivationDate >= dto.Package.ActivationDate
            && x.UnitCode == unitCode);

        return count >= unit!.RenterSlots;
    }

    public async Task<bool> CheckCustomerUnitPackageVehicleLimitations(Guid CustomerId , int numberOfVehicles)
    {
      var customerUnitPackge = await _repository.FirstOrDefaultAsync(x=>x.CustomerId == CustomerId);
        if (customerUnitPackge == null) {
            throw new Exception("NotFound");
            
        }
        return customerUnitPackge!.NumberOfVehicles > numberOfVehicles;
    }

    public async Task<bool> CheckPackageOverUnits(string UnitCode)
    {
        return await _repository.AnyAsync(x => x.UnitCode == UnitCode && x.EnglishName == "Owner Package");
    }


    #endregion

}

using SmartParking.Cutomers;
using SmartParking.Entities.CustomerUnitPackages;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Units;
using SmartParking.Entities.UnitVehiclePackges;
using SmartParking.Enums;
using SmartParking.Results;
using SmartParking.UnitPackages;
using SmartParking.UnitVehiclePackages;
using SmartParking.Utilities.StaticValues;
using SmartParking.Validations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.ObjectMapping;

namespace SmartParking.CustomerUnitPackages;

public class CustomerUnitPackageAppService : 
    CrudAppService<CustomerUnitPackage,CustomerUnitPackageDto, int, PagedAndSortedResultRequestDto, CreateUpdateCustomerUnitPackageDto>,
    ICustomerUnitPackageAppService
    //ReadOnlyAppService<CustomerUnitPackage, CustomerUnitPackageDto, int>,

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

    public override async Task DeleteAsync(int id)
    {
        var customerPackage = await _repository.FindAsync(id);

        if (customerPackage is null)
        {
            throw new EntityNotFoundException(typeof(CustomerPackage), id);
        }
        customerPackage.Status = Status.Deleted;
        await _repository.UpdateAsync(customerPackage, true);
    }

    public override async Task<CustomerUnitPackageDto> UpdateAsync(int id, CreateUpdateCustomerUnitPackageDto input)
    {
        return await base.UpdateAsync(id, input);
    }

    public async Task<Result<List<CustomerUnitPackageDto>>> GetListOfUnitPackageByCustomer(Guid customerId , int organizationId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _repository.GetQueryableAsync();
            query = query
                .Where(c => c.Customer.Id == customerId && c.OrganizationId==organizationId && c.Status != Status.Deleted)
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

    public async Task<bool> MultipuleCustomerRentalValidation(Guid CustomerId, UnitPackageDto dto, string unitCode)
    {
        return await _repository.AnyAsync(x => x.CustomerId != CustomerId
        && x.DectivationDate >= dto.Package.ActivationDate
        && x.UnitCode == unitCode
        && x.EnglishName != PackageStaticValues.EnglishName
        && x.Status != Status.Deleted);
    }

    public async Task<bool> IfCustomerHasMultiplePackageAsync(string UnitCode, UnitPackageDto dto)
    {
        return await _repository.AnyAsync(x => x.UnitCode == UnitCode
            && x.EnglishName != PackageStaticValues.EnglishName
            && x.ActivationDate <= dto.Package.DectivationDate
            && x.DectivationDate >= dto.Package.ActivationDate
            && x.Status != Status.Deleted);
    }
    public async Task<bool> CheckRenterSlotsOverUnit(UnitPackageDto dto, string unitCode)
    {

        var unit = await _unitRepository.FirstOrDefaultAsync(x => x.Code == unitCode);

        var count = await _repository.CountAsync(x => x.ActivationDate <= dto.Package.DectivationDate
            && x.DectivationDate >= dto.Package.ActivationDate
            && x.UnitCode == unitCode
            && x.Status != Status.Deleted);

        return count > unit!.RenterSlots;
    }

    public async Task<bool> CheckCustomerUnitPackageVehicleLimitations(Guid CustomerId, int numberOfVehicles)
    {
        var customerUnitPackge = await _repository.FirstOrDefaultAsync(x => x.CustomerId == CustomerId);
        if (customerUnitPackge == null)
        {
            throw new Exception("NotFound");

        }
        return customerUnitPackge!.NumberOfVehicles > numberOfVehicles;
    }

    public async Task<Result<CustomerUnitPackageDto>> GetCustomerUnitPackageByCustomerIdAsync(Guid id)
    {
        
            var customerUnitPackage = await _repository.FirstOrDefaultAsync(x => x.CustomerId == id && x.Status != Status.Deleted);
            if (customerUnitPackage == null)
            {
                return Result.NotFound("Customer unit package not found");
            }
            var customerUnitPackageDto = ObjectMapper.Map<CustomerUnitPackage, CustomerUnitPackageDto>(customerUnitPackage);
            return Result<CustomerUnitPackageDto>.Success(customerUnitPackageDto);
        
    }
    public async Task<bool> CheckPackageOverUnits(string UnitCode)
    {
        return await _repository.AnyAsync(x => x.UnitCode == UnitCode && x.EnglishName == "Owner Package" && x.Status != Status.Deleted);
    }

    public async Task<PagedResultDto<CustomerUnitPackageDto>> GetAllByCustomerId(Guid customerId, PagedAndSortedResultRequestDto input, bool includeDeleted, bool onlyActive = false)
    {
        var query = await _repository.GetQueryableAsync();
        query = query.Where(x => x.CustomerId == customerId);
        if (!includeDeleted)
        {
            query = query.Where(x => x.Status != Status.Deleted);
        }
        if (onlyActive) 
            query = query.Where(x => x.ActivationDate <= DateTime.Now && x.DectivationDate >= DateTime.Now && x.Status == Status.Active);
        var totalCount = await AsyncExecuter.CountAsync(query);
        query = query
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount);
        var packages = await AsyncExecuter.ToListAsync(query);
        var packagesDto = ObjectMapper.Map<List<CustomerUnitPackage>, List<CustomerUnitPackageDto>>(packages);
        return new PagedResultDto<CustomerUnitPackageDto>(
            totalCount,
            packagesDto
        );
    }


    public async Task<PagedResultDto<CustomerUnitPackageDto>> GetAllPronightPackages(PagedAndSortedResultRequestDto input)
    {
        var query = await _repository.GetQueryableAsync();
        query = query.Where(x => x.EnglishName != null);
        var totalCount = await AsyncExecuter.CountAsync(query);
        query = query
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount);
        var packages = await AsyncExecuter.ToListAsync(query);
        var packagesDto = ObjectMapper.Map<List<CustomerUnitPackage>, List<CustomerUnitPackageDto>>(packages);
        return new PagedResultDto<CustomerUnitPackageDto>(
            totalCount,
            packagesDto
        );
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

    

    #endregion

}

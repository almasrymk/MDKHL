using Microsoft.EntityFrameworkCore;
using SmartParking.Cameras;
using SmartParking.CustomerPackages.Dtos;
using SmartParking.CustomerUnitPackages;
using SmartParking.Entities.Cameras.Watchers;
using SmartParking.Entities.UnitVehiclePackges;
using SmartParking.Enums;
using SmartParking.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.UnitVehiclePackages;

public class UnitVehiclePackageAppService : CrudAppService<
    UnitVehiclePackge,
    UnitVehiclePackageDto,
    Guid,
    PagedAndSortedResultRequestDto,
    CreateUpdateUnitVehiclePackageDto>,
    IUnitVehiclePackageAppService
{
    private readonly ICustomerUnitPackageAppService _customerUnitPackageAppService;
    private readonly IRepository<UnitVehiclePackge, Guid> _repository;
    public UnitVehiclePackageAppService(IRepository<UnitVehiclePackge, Guid> repository,
        IVehiclesAppService vehiclesAppService,
        ICustomerUnitPackageAppService customerUnitPackageAppService)
        : base(repository)
    {
        _customerUnitPackageAppService = customerUnitPackageAppService;
        _repository = repository;
    }

    #region public methods

    public override async Task<UnitVehiclePackageDto> CreateAsync(CreateUpdateUnitVehiclePackageDto input)
    {
        var customerUnitPackage = await _customerUnitPackageAppService.GetAsync(input.CustomerUnitPackageId);
        //HandleCreateInput(input, customerUnitPackage);
        var query = await Repository.GetQueryableAsync();
        var packageBenificeriesNumber = await query.CountAsync(x => x.CustomerUnitPackageId == input.CustomerUnitPackageId);
            if (packageBenificeriesNumber > customerUnitPackage.NumberOfSlots)
        {
            throw new Exception("You have reached the maximum number of slots for this package");
        }
        var customerVehiclesCount = await query.CountAsync(x => x.CustomerId == input.CustomerId);
        return await base.CreateAsync(input);

    }

    public override async Task<UnitVehiclePackageDto> UpdateAsync(Guid id, CreateUpdateUnitVehiclePackageDto input)
    {
        
        var customerUnitPackage = await _customerUnitPackageAppService.GetAsync(input.CustomerUnitPackageId);
        //HandleCreateInput(input, customerUnitPackage);
        var query = await Repository.GetQueryableAsync();
        var packageBenificeriesNumber = await query.CountAsync(x => x.CustomerUnitPackageId == input.CustomerUnitPackageId && x.Id != id);
        if (packageBenificeriesNumber > customerUnitPackage.NumberOfSlots)
        {
            throw new Exception("You have reached the maximum number of slots for this package");
        }
        var customerVehiclesCount = await query.CountAsync(x => x.CustomerId == input.CustomerId && x.Id != id);
        if (customerVehiclesCount > customerUnitPackage.NumberOfVehicles)
        {
            throw new Exception("You have reached the maximum number of vehicles for this package");
        }
        return await base.UpdateAsync(id, input);
    }


    public async Task<bool> CheckDuplicationForVehicle(Guid CustomerId, string EnPlateNumber, string UnitCode)
    {
         return await _repository.AnyAsync(x => x.EnPlateNumber == EnPlateNumber 
         && x.CustomerId == CustomerId 
         && x.UnitCode == UnitCode);
    }

    public async Task<PagedResultDto<UnitVehiclePackageDto>> GetAllByCustomerId(Guid customerId, PagedAndSortedResultRequestDto input)
    {
        var query = await _repository.GetQueryableAsync();
        query = query.Where(x => x.CustomerId == customerId);
        var totalCount = await AsyncExecuter.CountAsync(query);
        query = query
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount);
        var packages = await AsyncExecuter.ToListAsync(query);
        var packagesDto = ObjectMapper.Map<List<UnitVehiclePackge>, List<UnitVehiclePackageDto>>(packages);
        return new PagedResultDto<UnitVehiclePackageDto>(
            totalCount,
            packagesDto
        );
    }

    public async Task DeleteByCustomerUnitPackage(int id)
    {
        var result = await _repository.FirstOrDefaultAsync(x => x.CustomerUnitPackageId == id);
        if (result is null)
            return;
        await base.DeleteAsync(result.Id);
    }

    public async Task<UnitVehiclePackageDto> GetByCustomerUnitPackageId(int id)
    {
        var result = await _repository.FirstOrDefaultAsync(x => x.CustomerUnitPackageId == id);
        return ObjectMapper.Map<UnitVehiclePackge, UnitVehiclePackageDto>(result!);
    }

    public async Task<UnitVehiclePackageDto> GetByCustmoerIdAndPlateNumber(SearchByPlateAndCustomerIdDto input)
    {
        var result = await _repository.FirstOrDefaultAsync(x => x.CustomerId == input.CustomerId && x.EnPlateNumber == input.EnPlateNumber && x.IsDeleted == false);
        return ObjectMapper.Map<UnitVehiclePackge, UnitVehiclePackageDto>(result!);
    }

    public async Task<UnitVehiclePackageDto> GetUnitVehiclePackageByPackageCode(string packageCode)
    {
        var result = await _repository.FirstOrDefaultAsync(x => x.PackageCode == packageCode && x.IsDeleted == false);
        return ObjectMapper.Map<UnitVehiclePackge, UnitVehiclePackageDto>(result!);
    }

    #endregion

    #region private methods

    private void HandleCreateInput(CreateUpdateUnitVehiclePackageDto input, CustomerUnitPackageDto customerUnitPackage)
    {
        input.ActivationDate = customerUnitPackage.ActivationDate;
        input.DectivationDate = customerUnitPackage.DectivationDate;
        input.PublishFrom = customerUnitPackage.PublishFrom;
        input.PublishTo = customerUnitPackage.PublishTo;
        input.UnitCode = customerUnitPackage.UnitCode;
    }


    #endregion

}

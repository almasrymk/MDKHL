using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartParking.CustomerPackages.Dtos;
using SmartParking.CustomerUnitPackages;
using SmartParking.Entities.Organizations;
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
    private readonly IRepository<CustomerPackage, int> _customerPackageRepository;
    public UnitVehiclePackageAppService(IRepository<UnitVehiclePackge, Guid> repository,
        IVehiclesAppService vehiclesAppService,
        ICustomerUnitPackageAppService customerUnitPackageAppService,
        IRepository<CustomerPackage, int> customerPackageRepository)
        : base(repository)
    {
        _customerUnitPackageAppService = customerUnitPackageAppService;
        _repository = repository;
        _customerPackageRepository = customerPackageRepository;
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
        await HandleExistingPackagesForVehicleAsync(input);
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


    public async Task<bool> CheckDuplicationForVehicle(string EnPlateNumber, int customerUnitPackageId)
    {
         return await _repository.AnyAsync(x => x.EnPlateNumber == EnPlateNumber 
         && x.CustomerUnitPackageId == customerUnitPackageId && x.IsDeleted==false);
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
        using (DataFilter.Disable<IMultiTenant>())
        {

            var result = await _repository.GetListAsync(x => x.CustomerUnitPackageId == id);
            if (result is null)
                return;
            foreach (var item in result)
                await base.DeleteAsync(item.Id);
        }
    }
    [HttpGet("api/app/unit-vehicle-package/{id}/single-by-customer")]
    public async Task<List<UnitVehiclePackageDto>> GetByCustomerUnitPackageId(int id)
    {
        var result = await _repository.GetListAsync(x => x.CustomerUnitPackageId == id);
        return ObjectMapper.Map<List<UnitVehiclePackge>, List<UnitVehiclePackageDto>>(result);
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
    [HttpGet("api/app/unit-vehicle-package/{id}/all-by-customer")]
    public async Task<List<UnitVehiclePackageDto>> GetAllByCustomerUnitPackageId(int id)
    {
        var query = await _repository.GetQueryableAsync();
        query = query.Where(x => x.CustomerUnitPackageId == id);
        var result = await AsyncExecuter.ToListAsync(query);
        return ObjectMapper.Map<List<UnitVehiclePackge>, List<UnitVehiclePackageDto>>(result);
    }


    public async Task DeleteByPlateNumber(string EnPlateNumber)
    {
        await _repository.DeleteAsync(x => x.EnPlateNumber == EnPlateNumber);
    }


    #endregion

    #region private methods

    private async Task HandleExistingPackagesForVehicleAsync(CreateUpdateUnitVehiclePackageDto dto)
    {
        var newExpiryDate = dto.ActivationDate.AddDays(-1);
        var newActivationDate = dto.DectivationDate.AddDays(1);

        var query = await _repository.GetQueryableAsync();

        var ids = query
            .Where(p =>
                p.UnitCode == dto.UnitCode &&
                p.EnPlateNumber == dto.EnPlateNumber &&
                (
                    p.ActivationDate <= dto.DectivationDate &&   // Existing starts before new one ends
                    p.DectivationDate >= dto.ActivationDate      // Existing ends after new one starts
                ) 
            )
            .Select(p => p.CustomerPackgeId)
            .Distinct();

            

        var cpQuery = await _customerPackageRepository.GetQueryableAsync();

        await cpQuery
            .Where(p => ids.Contains(p.Id))
            .ExecuteUpdateAsync(p => p

            // 1. Update Status to Inactive ONLY if the Activation Dates match
            .SetProperty(x => x.CustomerPackageStatus,
                x => x.ActivationDateOfPackage == dto.ActivationDate
                    ? CustomerPackageStatus.Expired
                    : x.CustomerPackageStatus)

            .SetProperty(x => x.Status,
                x => x.ActivationDateOfPackage == dto.ActivationDate
                    ? Status.Inactive
                    : x.Status)

            // 2. Update ExpiryDate if the existing package is strictly BEFORE the new one
            .SetProperty(x => x.ExpiryDate,
                x => x.ActivationDateOfPackage <= dto.ActivationDate
                    ? newExpiryDate
                    : x.ExpiryDate)

            // 3. Update ActivationDate if the existing package is strictly AFTER the new one
            .SetProperty(x => x.ActivationDateOfPackage,
                x => x.ActivationDateOfPackage > dto.ActivationDate
                    ? (newActivationDate < x.DectivationDateOfPackage ? newActivationDate : x.DectivationDateOfPackage)
                    : x.ActivationDateOfPackage)
        );

        await query
            .Where(p => ids.Contains(p.CustomerPackgeId)
            )
            .ExecuteUpdateAsync(p => p

                .SetProperty(x => x.DectivationDate,
                    x => x.ActivationDate < dto.ActivationDate
                        ? newExpiryDate
                        : x.DectivationDate)

                .SetProperty(x => x.ActivationDate,
                    x => x.ActivationDate > dto.ActivationDate
                        ? (newActivationDate < x.DectivationDate ? newActivationDate : x.DectivationDate)
                        : x.ActivationDate)
            );

    }


    #endregion

}

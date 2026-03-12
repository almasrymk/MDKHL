using Microsoft.EntityFrameworkCore;
using SmartParking.CustomerUnitPackages;
using SmartParking.Entities.UnitVehiclePackges;
using SmartParking.Vehicles;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

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
        if (customerVehiclesCount > customerUnitPackage.NumberOfVehicles)
        {
            throw new Exception("You have reached the maximum number of vehicles for this package");
        }
        return await base.CreateAsync(input);

    }

    

    public async Task<bool> CheckDuplicationForVehicle(Guid CustomerId, string EnPlateNumber)
    {
         return await _repository.AnyAsync(x => x.EnPlateNumber == EnPlateNumber && x.CustomerId == CustomerId);
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

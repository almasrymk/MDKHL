using Abp.Application.Services;
using SmartParking.CustomerPackages.Dtos;
using SmartParking.CustomerUnitPackages;
using SmartParking.Entities.GarageTransactions;
using SmartParking.GarageTransactions;
using SmartParking.Repositories;
using SmartParking.UnitVehiclePackages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.CustomerPackages.CustomerPackageTransactions;
[RemoteService(false)]
public class CustomerPackageHandlerAppService : ICustomerPackageHandlerAppService
{
    private readonly IEnumerable<ICustomerPackageTransactionActionsAppService> _services;
    private readonly ICustomerUnitPackageAppService _customerUnitPackageAppService;
    private readonly IGarageTransactionValidationsRepository _garageTransactionValidationsRepository;
    private readonly IDataFilter _dataFilter;
    private readonly IRepository<GarageTransaction, int> _garageTransactionRepository;
    private readonly IUnitVehiclePackageAppService _unitVehiclePackageAppService;

    public CustomerPackageHandlerAppService(IEnumerable<ICustomerPackageTransactionActionsAppService> services,
        ICustomerUnitPackageAppService customerUnitPackageAppService,
        IGarageTransactionValidationsRepository garageTransactionValidationsRepository,
        IDataFilter dataFilter,
        IRepository<GarageTransaction, int> garageTransactionRepository,
        IUnitVehiclePackageAppService unitVehiclePackageAppService)
    {
        _services = services;
        _customerUnitPackageAppService = customerUnitPackageAppService;
        _garageTransactionValidationsRepository = garageTransactionValidationsRepository;
        _dataFilter = dataFilter;
        _garageTransactionRepository = garageTransactionRepository;
        _unitVehiclePackageAppService = unitVehiclePackageAppService;
    }

    public async Task<bool> CheckIfCustomerPackageHasAvailableSlots(Guid customerId, int organizationId, string packageCode)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {

            var unit = await _unitVehiclePackageAppService.GetUnitVehiclePackageByPackageCode(packageCode);

            var unitPackagesResult = await _customerUnitPackageAppService.GetListOfUnitPackageByCustomer(customerId, organizationId);
            var unitPackages = unitPackagesResult.Value; 

            if (unitPackages == null || !unitPackages.Any())
            {
                return false;
            }

            var occupancyList = await _garageTransactionValidationsRepository.GetCountOfVehiclesInsideByUnitAsync(customerId, organizationId);
            var currentOccupancy = occupancyList
                    .FirstOrDefault(x => x.UnitCode == unit.UnitCode)?.VehicleCount ?? 0;
            return (unitPackages.Where(x => x.UnitCode == unit.UnitCode).FirstOrDefault().NumberOfSlots >= currentOccupancy + 1);
        }
    }

    public async Task<bool> CheckIfVehicleIsInside(string PlateNumber)
    {
        var query = await _garageTransactionRepository.GetQueryableAsync();
        var isVehicleInside = query.Any(x => x.PlateNumber == PlateNumber && x.IsIn);
        return isVehicleInside;
    }

    public async Task<CustomerPackageTransactionDto> GetOneActiveCustomerPackage(GarageTransactionDto garageTransactionDto)
    {
        foreach (var action in _services) {
            if (action.CanHandle(garageTransactionDto))
            {
               return await action.GetOneActiveCustomerPackage(garageTransactionDto);
            }
        
        }
        return null;
    }

}

using Abp.Application.Services;
using SmartParking.CustomerPackages.Dtos;
using SmartParking.CustomerUnitPackages;
using SmartParking.Entities.GarageTransactions;
using SmartParking.GarageTransactions;
using SmartParking.Repositories;
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

    public CustomerPackageHandlerAppService(IEnumerable<ICustomerPackageTransactionActionsAppService> services, 
        ICustomerUnitPackageAppService customerUnitPackageAppService, 
        IGarageTransactionValidationsRepository garageTransactionValidationsRepository, 
        IDataFilter dataFilter, 
        IRepository<GarageTransaction, int> garageTransactionRepository)
    {
        _services = services;
        _customerUnitPackageAppService = customerUnitPackageAppService;
        _garageTransactionValidationsRepository = garageTransactionValidationsRepository;
        _dataFilter = dataFilter;
        _garageTransactionRepository = garageTransactionRepository;
    }

    public async Task<bool> CheckIfCustomerPackageHasAvailableSlots(Guid customerId, int organizationId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {

            // 1. Get the list of all units and their slot capacities
            var unitPackagesResult = await _customerUnitPackageAppService.GetListOfUnitPackageByCustomer(customerId, organizationId);
            var unitPackages = unitPackagesResult.Value; // Assuming .Value is List<UnitPackageDto>

            if (unitPackages == null || !unitPackages.Any())
            {
                return false;
            }

            // 2. Get the current occupancy (cars inside) grouped by UnitCode
            // This method should return List<UnitOccupancyDto> { UnitCode, VehicleCount }
            var occupancyList = await _garageTransactionValidationsRepository.GetCountOfVehiclesInsideByUnitAsync(customerId, organizationId);

            // 3. Check each unit to see if at least one has an available slot
            foreach (var unit in unitPackages)
            {
                // Find how many cars are currently inside for THIS specific unit
                var currentOccupancy = occupancyList
                    .FirstOrDefault(x => x.UnitCode == unit.UnitCode)?.VehicleCount ?? 0;

                if (unit.NumberOfSlots >= (currentOccupancy + 1))
                {
                    continue;
                }
                else
                {
                    return false;
                }
            }

            return true;
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

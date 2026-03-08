using Abp.Application.Services;
using SmartParking.CustomerPackages.Dtos;
using SmartParking.CustomerUnitPackages;
using SmartParking.GarageTransactions;
using SmartParking.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;

namespace SmartParking.CustomerPackages.CustomerPackageTransactions;
[RemoteService(false)]
public class CustomerPackageHandlerAppService : ICustomerPackageHandlerAppService
{
    private readonly IEnumerable<ICustomerPackageTransactionActionsAppService> _services;
    private readonly ICustomerUnitPackageAppService _customerUnitPackageAppService;
    private readonly IGarageTransactionValidationsRepository _garageTransactionValidationsRepository;
    private readonly IDataFilter _dataFilter;

    public CustomerPackageHandlerAppService(IEnumerable<ICustomerPackageTransactionActionsAppService> services, ICustomerUnitPackageAppService customerUnitPackageAppService, IGarageTransactionValidationsRepository garageTransactionValidationsRepository, IDataFilter dataFilter)
    {
        _services = services;
        _customerUnitPackageAppService = customerUnitPackageAppService;
        _garageTransactionValidationsRepository = garageTransactionValidationsRepository;
        _dataFilter = dataFilter;
    }

    public async Task<bool> CheckIfCustomerPackageHasAvailableSlots(Guid customerId, int organizationId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {

<<<<<<< HEAD
<<<<<<< HEAD
        var customerUnitPackage =await _customerUnitPackageAppService.GetListOfUnitPackageByCustomer(customerId, organizationId);
        var garageTransactionValidation = await _garageTransactionValidationsRepository.CountOfVehicleRelatedToCustomerPackageParkingInGarage(customerId, organizationId);

        return customerUnitPackage.Value[0].NumberOfSlots >= ((garageTransactionValidation?.Count+1) ?? 0);
=======
=======
>>>>>>> f7dfad3fd794f0105bfbec31fa5b53ad5edf60ad
        var customerUnitPackage =await _customerUnitPackageAppService.GetUnitPackageByCustomer(customerId);
        var garageTransactionValidation = await _garageTransactionValidationsRepository.CountOfVehicleRelatedToCustomerPackageParkingInGarage(customerId, organizationId);

        return customerUnitPackage.NumberOfSlots >= ((garageTransactionValidation?.Count+1) ?? 0);
<<<<<<< HEAD
>>>>>>> f7dfad3fd794f0105bfbec31fa5b53ad5edf60ad
=======
>>>>>>> f7dfad3fd794f0105bfbec31fa5b53ad5edf60ad
        }
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

using SmartParking.CustomerPackages.Dtos;
using SmartParking.GarageTransactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;

namespace SmartParking.CustomerPackages.CustomerPackagesTransaction;

[RemoteService(false)]
public class CustomerPackageHandlerAppService : ICustomerPackageHandlerAppService
{
    private readonly IEnumerable<ICustomerPackageTransactionActionsAppService> _services;

    public CustomerPackageHandlerAppService(IEnumerable<ICustomerPackageTransactionActionsAppService> services)
    {
        _services = services;
    }

    public async Task<CustomerPackageTransactionDto> GetOneActiveCustomerPackage(GarageTransactionDto garageTransactionDto)
    {
        foreach (var action in _services)
        {
            if (action.CanHandle(garageTransactionDto))
            {
                return await action.GetOneActiveCustomerPackage(garageTransactionDto);
            }

        }
        return null;
    }
}

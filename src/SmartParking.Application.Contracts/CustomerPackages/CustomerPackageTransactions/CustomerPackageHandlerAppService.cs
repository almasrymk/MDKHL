using Abp.Application.Services;
using SmartParking.GarageTransactions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartParking.CustomerPackages.CustomerPackageTransactions;
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
        foreach (var action in _services) {
            if (action.CanHandle(garageTransactionDto))
            {
               return await action.GetOneActiveCustomerPackage(garageTransactionDto);
            }
        
        }
        return null;
    }
}

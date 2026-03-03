using SmartParking.CustomerPackages.Dtos;
using SmartParking.GarageTransactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.CustomerPackages.CustomerPackagesTransaction;

public interface ICustomerPackageTransactionActionsAppService
{
    Task<CustomerPackageTransactionDto> GetOneActiveCustomerPackage(GarageTransactionDto garageTransactionDto);
    bool CanHandle(GarageTransactionDto garageTransactionDto);

}

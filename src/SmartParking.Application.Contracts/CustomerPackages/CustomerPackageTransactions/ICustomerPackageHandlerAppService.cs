using Abp.Application.Services;
using SmartParking.GarageTransactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.CustomerPackages.CustomerPackageTransactions;

[RemoteService (false)]
public interface ICustomerPackageHandlerAppService
{
    Task<CustomerPackageTransactionDto> GetOneActiveCustomerPackage(GarageTransactionDto garageTransactionDto);
}

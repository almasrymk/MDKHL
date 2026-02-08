using SmartParking.GarageTransactions;
using System.Threading.Tasks;
using Volo.Abp;

namespace SmartParking.CustomerPackages.CustomerPackagesTransaction;

[RemoteService(false)]
public interface ICustomerPackageHandlerAppService
{
    Task<CustomerPackageTransactionDto> GetOneActiveCustomerPackage(GarageTransactionDto garageTransactionDto);
}

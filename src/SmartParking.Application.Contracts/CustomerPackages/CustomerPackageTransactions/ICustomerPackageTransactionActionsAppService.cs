using SmartParking.CustomerPackages.Dtos;
using SmartParking.GarageTransactions;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartParking.CustomerPackages.CustomerPackageTransactions;

public interface ICustomerPackageTransactionActionsAppService : IApplicationService
{
    Task<CustomerPackageTransactionDto> GetOneActiveCustomerPackage(GarageTransactionDto garageTransactionDto);
    bool CanHandle(GarageTransactionDto garageTransactionDto);

}

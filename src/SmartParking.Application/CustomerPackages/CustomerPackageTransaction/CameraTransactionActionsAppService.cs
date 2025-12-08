using Abp.Application.Services;
using Microsoft.EntityFrameworkCore;
using SmartParking.CustomerPackages.CustomerPackageTransactions;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.GarageTransactions;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.CustomerPackages.CustomerPackageTransaction;
[RemoteService(false)]

public class CameraTransactionActionsAppService : SmartParkingAppService ,  ICustomerPackageTransactionActionsAppService
{
    private readonly IDataFilter _dataFilter;
    private readonly IRepository<CustomerPackage, int> _repository;

    public CameraTransactionActionsAppService(IRepository<CustomerPackage, int> repository, IDataFilter dataFilter)
    {
        _repository = repository;
        _dataFilter = dataFilter;
    }

    public bool CanHandle(GarageTransactionDto garageTransactionDto)
    {
        return !string.IsNullOrEmpty(garageTransactionDto.PlateNumber) && string.IsNullOrEmpty(garageTransactionDto.TicketId);

    }

    public async Task<CustomerPackageTransactionDto> GetOneActiveCustomerPackage(GarageTransactionDto garageTransactionDto)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _repository.GetQueryableAsync();
            query = query.Where(x => x.OrganizationId == garageTransactionDto.OrganizationID && (x.ArPlateNumber == garageTransactionDto.PlateNumber || x.EnPlateNumber== garageTransactionDto.PlateNumber) && x.CustomerPackageStatus == CustomerPackageStatus.Active && x.ExpiryDate >= garageTransactionDto.AccurateTime && ((x.IsCustomPackage && x.PackageId == null) || (!x.IsCustomPackage && x.PackageId != null)));
            var customerPackageDto = await query.FirstOrDefaultAsync();
            return ObjectMapper.Map<CustomerPackage, CustomerPackageTransactionDto>(customerPackageDto);


        }
    }
}

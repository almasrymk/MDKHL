using Microsoft.EntityFrameworkCore;
using SmartParking.Entities.Organizations;
using SmartParking.GarageTransactions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.CustomerPackages.CustomerPackagesTransaction;

[RemoteService(false)]

public class DispenserTransactionActionsAppService : SmartParkingAppService, ICustomerPackageTransactionActionsAppService
{
    private readonly IDataFilter _dataFilter;
    private readonly IRepository<CustomerPackage, int> _repository;

    public DispenserTransactionActionsAppService(IRepository<CustomerPackage, int> repository, IDataFilter dataFilter)
    {
        _repository = repository;
        _dataFilter = dataFilter;
    }

    public bool CanHandle(GarageTransactionDto garageTransactionDto)
    {
        return !string.IsNullOrEmpty(garageTransactionDto.TicketId);
    }

    public async Task<CustomerPackageTransactionDto> GetOneActiveCustomerPackage(GarageTransactionDto garageTransactionDto)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            if (garageTransactionDto == null || string.IsNullOrEmpty(garageTransactionDto.TicketId) || garageTransactionDto.AccurateTime == null)
            {
                return null!;
            }

            var accurateTime = garageTransactionDto.AccurateTime.Value;
            var timeOfDay = TimeOnly.FromTimeSpan(accurateTime.TimeOfDay);

            using (_dataFilter.Disable<IMultiTenant>())
            {
                var query = await _repository.GetQueryableAsync();

                query = query.Where(x =>
                    x.OrganizationId == garageTransactionDto.OrganizationID &&
                    x.PackageCode == garageTransactionDto.TicketId &&

                    x.ActivationDateOfPackage.HasValue &&
                    x.DectivationDateOfPackage.HasValue &&
                    x.ActivationDateOfPackage.Value <= accurateTime &&
                    x.DectivationDateOfPackage.Value >= accurateTime &&

                    x.PublishFromOfPackage.HasValue &&
                    x.PublishToOfPackage.HasValue &&
                    (
                        (x.PublishFromOfPackage.Value <= x.PublishToOfPackage.Value &&
                         x.PublishFromOfPackage.Value <= timeOfDay &&
                         x.PublishToOfPackage.Value >= timeOfDay)
                        ||
                        //midnight
                        (x.PublishFromOfPackage.Value > x.PublishToOfPackage.Value &&
                         (x.PublishFromOfPackage.Value <= timeOfDay || x.PublishToOfPackage.Value >= timeOfDay))
                    ) &&

                    ((x.IsCustomPackage && x.PackageId == null) || (!x.IsCustomPackage && x.PackageId != null))
                );
                var customerPackageDto = await query.FirstOrDefaultAsync();
                return ObjectMapper.Map<CustomerPackage, CustomerPackageTransactionDto>(customerPackageDto);
            }
        }
    }
}

using Abp.Application.Services;
using SmartParking.Entities.Organizations;
using SmartParking.GarageTransactions;
using System.Linq;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.CustomerPackages.CustomerPackageTransaction;
[RemoteService(false)]

public class DispenserTransactionActionsAppService : CustomerPackageTransactionBaseAppService
{


    public DispenserTransactionActionsAppService(IRepository<CustomerPackage, int> repository, 
        IDataFilter dataFilter) : base(repository, dataFilter)
    {
    }



    /// <summary>
    /// Dispenser transactions use ticket/package codes
    /// </summary>
    public override bool CanHandle(GarageTransactionDto garageTransactionDto)
    {
        return !string.IsNullOrEmpty(garageTransactionDto.TicketId);
    }

    /// <summary>
    /// Override to filter only by ticket/package code for dispenser transactions
    /// </summary>
    protected override IQueryable<CustomerPackage> ApplyIdentificationFilters(
        IQueryable<CustomerPackage> query,
        GarageTransactionDto garageTransactionDto)
    {
        return query.Where(x => x.PackageCode == garageTransactionDto.TicketId);
    }

    /// <summary>
    /// Dispenser-specific validation
    /// </summary>
    protected override bool ValidateGarageTransactionDto(GarageTransactionDto garageTransactionDto)
    {
        return base.ValidateGarageTransactionDto(garageTransactionDto) &&
               !string.IsNullOrEmpty(garageTransactionDto.TicketId);
    }
}

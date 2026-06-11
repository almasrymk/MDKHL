using SmartParking.Entities.Organizations;
using SmartParking.GarageTransactions;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace SmartParking.CustomerPackages.CustomerPackageTransaction;
[RemoteService(false)]

public class CameraTransactionActionsAppService : CustomerPackageTransactionBaseAppService
{


    public CameraTransactionActionsAppService(IRepository<CustomerPackage, int> repository, 
        IDataFilter dataFilter)
        : base(repository, dataFilter)
    {
    }
    

    /// <summary>
    /// Camera transactions use plate numbers only
    /// </summary>
    public override bool CanHandle(GarageTransactionDto garageTransactionDto)
    {
        return !string.IsNullOrEmpty(garageTransactionDto.PlateNumber) &&
               string.IsNullOrEmpty(garageTransactionDto.TicketId);
    }

    /// <summary>
    /// Override to filter only by plate number for camera transactions
    /// </summary>
    protected override IQueryable<CustomerPackage> ApplyIdentificationFilters(
        IQueryable<CustomerPackage> query,
        GarageTransactionDto garageTransactionDto)
    {
        return query.Where(x =>
            x.ArPlateNumber == garageTransactionDto.PlateNumber ||
            x.EnPlateNumber == garageTransactionDto.PlateNumber
        );
    }

    /// <summary>
    /// Camera-specific validation
    /// </summary>
    protected override bool ValidateGarageTransactionDto(GarageTransactionDto garageTransactionDto)
    {
        return base.ValidateGarageTransactionDto(garageTransactionDto) &&
               !string.IsNullOrEmpty(garageTransactionDto.PlateNumber);
    }
}

using SmartParking.Entities.GarageTransactions;
using SmartParking.Enums;
using SmartParking.GarageTransactions;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.GarageTransactionEndPoints.Factory;

public class GarageTransactionsDispenserActionsAppService : IGarageTransactionActionsAppService
{
    private readonly IRepository<GarageTransaction, int> _repository;

    public GarageTransactionsDispenserActionsAppService(IRepository<GarageTransaction, int> repository)
    {
        _repository = repository;
    }


    public async Task UnMappingEntryTransactionForDeletedExit(GarageTransactionDto garageTransactionDto)
    {
        var query = await _repository.GetQueryableAsync();

        var latestTransaction = query
                .Where(gt => gt.TicketId == garageTransactionDto.TicketId && gt.CameraType == WatcherType.Entry && gt.IsIn == false && gt.IsMapped == true)
                .OrderByDescending(gt => gt.AccurateTime)
                .SingleOrDefault();
        if (latestTransaction == null)
        {
            return;
        }
        latestTransaction.IsIn = true;
        latestTransaction.IsMapped = false;
    }

    public bool CanHandle(GarageTransactionDto garageTransactionDto)
    {
        return !string.IsNullOrEmpty(garageTransactionDto.TicketId);
    }
}

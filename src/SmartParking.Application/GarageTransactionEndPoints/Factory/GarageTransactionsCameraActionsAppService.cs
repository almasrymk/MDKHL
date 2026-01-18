using SmartParking.Entities.GarageTransactions;
using SmartParking.Enums;
using SmartParking.GarageTransactions;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.GarageTransactionEndPoints.Factory;

public class GarageTransactionsCameraActionsAppService : IGarageTransactionActionsAppService
{
    private readonly IRepository<GarageTransaction, int> _repository;
    public GarageTransactionsCameraActionsAppService(IRepository<GarageTransaction, int> repository)
    {
        _repository = repository;
    }
    public async Task UnMappingEntryTransactionForDeletedExit(GarageTransactionDto garageTransactionDto)
    {
        var query = await _repository.GetQueryableAsync();

        var latestTransaction = query
                .Where(gt => gt.PlateNumber == garageTransactionDto.PlateNumber && gt.CameraType == WatcherType.Entry && gt.IsIn == false && gt.IsMapped == true)
                .OrderByDescending(gt => gt.AccurateTime)
                .FirstOrDefault();
        if (latestTransaction == null) {
            return;
        }
        latestTransaction.IsIn = true;
        latestTransaction.IsMapped = false;
    }
    public bool CanHandle(GarageTransactionDto garageTransactionDto)
    {
        return !string.IsNullOrEmpty(garageTransactionDto.PlateNumber) && string.IsNullOrEmpty(garageTransactionDto.TicketId);
    }

}

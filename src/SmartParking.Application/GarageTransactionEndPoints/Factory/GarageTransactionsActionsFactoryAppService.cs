using SmartParking.GarageTransactions;
using SmartParking.GarageTransactions.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.GarageTransactionEndPoints.Factory;

public class GarageTransactionsActionsFactoryAppService : IGarageTransactionsActionsFactoryAppService
{
    private readonly IEnumerable<IGarageTransactionActionsAppService> _services;

    public GarageTransactionsActionsFactoryAppService(IEnumerable<IGarageTransactionActionsAppService> services)
    {
        _services = services;
    }

    public async Task<GarageTransactionDto> CheckIfCheckoutFromMobile(GarageTransactionDto garageTransactionDto)
    {
        var service = GetConsumerService(garageTransactionDto);
        var result= await service.CheckIfCheckoutFromMobile(garageTransactionDto);
        return result;

    }

    public async Task UnMappingEntryTransactionForDeletedExit(GarageTransactionDto garageTransactionDto)
    {
        var service = GetConsumerService(garageTransactionDto);
        await service.UnMappingEntryTransactionForDeletedExit(garageTransactionDto);
    }


    private IGarageTransactionActionsAppService GetConsumerService(GarageTransactionDto garageTransactionDto)
    {
        foreach (var service in _services)
        {
            if (service.CanHandle(garageTransactionDto))
            {
                return service;
            }
        }
        throw new Exception("Invalid Action!!");
    }

}

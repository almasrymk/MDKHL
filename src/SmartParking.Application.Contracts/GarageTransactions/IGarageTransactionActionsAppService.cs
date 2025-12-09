using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.GarageTransactions;

public interface IGarageTransactionActionsAppService
{
    /// <summary>
    /// method used to be implemented by all transactions consumers.
    /// </summary>
    /// <param name="key">each consumer key either (plate , ticketId , etc..)</param>
    Task UnMappingEntryTransactionForDeletedExit(GarageTransactionDto key);
    /// <summary>
    /// method used to be implemented by all transactions consumers to get amapped from mobile transaction.
    /// </summary>
    /// <param name="key">each consumer key either (plate , ticketId , etc..)</param>
    /// <returns>transaction that matched</returns>
    Task<GarageTransactionDto> CheckIfCheckoutFromMobile(GarageTransactionDto key);


    bool CanHandle(GarageTransactionDto garageTransactionDto);
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.GarageTransactions.Factory;

public interface IGarageTransactionsActionsFactoryAppService
{
    /// <summary>
    /// method used to unmap entry transaction for exit one
    /// </summary>
    /// <param name="garageTransactionDto">entry transaction</param>
    Task UnMappingEntryTransactionForDeletedExit(GarageTransactionDto garageTransactionDto);
}

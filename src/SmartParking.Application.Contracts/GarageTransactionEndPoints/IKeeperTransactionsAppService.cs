using SmartParking.Invoices;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartParking.GarageTransactionEndPoints;

public interface IKeeperTransactionsAppService : IApplicationService
{
    /// <summary>
    /// Method used to calculate invoice for keeper depends on transaction 
    /// </summary>
    /// <param name="entryTransactionId">Entry transaction Id</param>
    /// <returns>Invoice dto</returns>

    Task<Result<InvoiceDto>> Checkout(int entryTransactionId);

    /// <summary>
    /// Method used to calculate pre invoice for keeper on entry transaction
    /// </summary>
    /// <param name="entryTransactionId">Entry transaction Id</param>
    /// <returns>Pre invoice dto</returns>
    Task<Result<CreateUpdateInvoiceDto>> PreCheckout(int entryTransactionId);
}

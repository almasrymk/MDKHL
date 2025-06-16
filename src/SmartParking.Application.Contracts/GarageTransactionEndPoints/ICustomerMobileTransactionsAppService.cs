using AutoMapper.Internal.Mappers;
using SmartParking.Entities.GarageTransactions;
using SmartParking.GarageTransactions;
using SmartParking.Invoices;
using SmartParking.MobileInvoices;
using SmartParking.Results;
using SmartParking.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartParking.GarageTransactionEndPoints;

public interface ICustomerMobileTransactionsAppService : IApplicationService
{    
    
    /// <summary>
     /// Method used to get invoice for customer when he checks out
     /// </summary>
     /// <param name="entryTransactionId">Entry transaction Id</param>
     /// <returns>Invoice dto</returns>
    Task<Result<InvoiceDto>> Checkout(int garageTransactionId);
    /// <summary>
    /// Method used to calculate pre invoice for customer
    /// </summary>
    /// <param name="entryTransactionId">Entry transaction Id</param>
    /// <returns>Pre invoice dto</returns>
    Task<Result<CreateUpdateInvoiceDto>> PreCheckout(int garageTransactionId);


}

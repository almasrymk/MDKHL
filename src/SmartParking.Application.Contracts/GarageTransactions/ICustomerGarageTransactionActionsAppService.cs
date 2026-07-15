using SmartParking.Invoices;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartParking.GarageTransactions;

public interface ICustomerGarageTransactionActionsAppService : IApplicationService
{
    Task<Result<CreateUpdateInvoiceDto>> VehiclePreCheckoutAsync(string input);
    Task<Result<InvoiceDto>> VehicleCheckoutAsync(string input);

}

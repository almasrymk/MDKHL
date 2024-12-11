using SmartParking.CustomerPackages;
using SmartParking.Entities.Customers;
using SmartParking.GarageTransactions;
using SmartParking.Invoices;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartParking.GarageTransactionEndPoints;

public interface IGarageTransactionEndPointsAppService : IApplicationService
{
    Task<Boolean> CreateAsync(CreateUpdateGarageTransactionEndPointDto input);
    int CalculateTimeDependsOnPackage(DateTime? entranceTime, DateTime? exitTime, CustomerPackageDto packageDto, DateTime? PackageExpiryDate);
    Task<Result<InvoiceDto>> CustomerCheckoutFromHisMobile(int garageTransactionId);
    Task<Result<CreateUpdateInvoiceDto>> CustomerPreCheckoutFromHisMobile(int garageTransactionId);
    Task<Result<InvoiceDto>> CustomerCheckoutFromKeeperMobile(int garageTransactionId);
    Task<Result<CreateUpdateInvoiceDto>> CustomerPreCheckoutFromKeeperMobile(int garageTransactionId);
    
}

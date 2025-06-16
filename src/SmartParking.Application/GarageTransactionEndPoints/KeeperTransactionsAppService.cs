using Microsoft.AspNetCore.Authorization;
using SmartParking.CalculationsEndpoint;
using SmartParking.CustomerPackages;
using SmartParking.Entities.Customers;
using SmartParking.Entities.GarageTransactions;
using SmartParking.Enums;
using SmartParking.GarageTransactions;
using SmartParking.Invoices;
using SmartParking.Results;
using SmartParking.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.GarageTransactionEndPoints;

[Authorize]
public class KeeperTransactionsAppService : SmartParkingAppService, IKeeperTransactionsAppService
{
    private readonly IInvoiceAppService _invoiceAppService;
    private readonly ICalculationsEndPointsAppService _calculationsEndPointAppService;
    private readonly IGarageTransactionUtilityAppService _garageTransactionUtilityAppService;
    private readonly IGarageTransactionsAppService _garageTransactionsAppService;
    private readonly ICustomerPackagesAppService _customerPackagesAppService;

    public KeeperTransactionsAppService(IInvoiceAppService invoiceAppService, ICalculationsEndPointsAppService calculationsEndPointAppService, IGarageTransactionUtilityAppService garageTransactionUtilityAppService, IGarageTransactionsAppService garageTransactionsAppService, ICustomerPackagesAppService customerPackagesAppService)
    {
        _invoiceAppService = invoiceAppService;
        _calculationsEndPointAppService = calculationsEndPointAppService;
        _garageTransactionUtilityAppService = garageTransactionUtilityAppService;
        _garageTransactionsAppService = garageTransactionsAppService;
        _customerPackagesAppService = customerPackagesAppService;
    }


    #region public methods for keeper services

    ///<inheritdoc/>
    public async Task<Result<InvoiceDto>> Checkout(int entryTransactionId)
    {
        var entryGarageTransaction = await _garageTransactionUtilityAppService.GetTransactionById(entryTransactionId);
        var invoice = await PreCheckout(entryTransactionId);
        var lstCheckout = entryGarageTransaction.CheckoutMobile.HasValue ? entryGarageTransaction.CheckoutMobile.Value : DateTime.MinValue;
        var _garageTransactionDto = ObjectMapper.Map<GarageTransaction, GarageTransactionDto>(entryGarageTransaction);
        if (!(_garageTransactionDto.IsIn && _garageTransactionDto.IsPending == false && _garageTransactionDto.CanCheckout == true))
        {
            throw new Exception("Transaction Already Checked-Out");
        }
        var createdExitTransaction =await _garageTransactionUtilityAppService.InitiateExitGarageTransactionForMobileCheckout(entryGarageTransaction.PlateNumber, entryGarageTransaction.VehiclePicture, entryGarageTransaction.OrganizationID, entryGarageTransaction.GarageId, entryGarageTransaction.CameraId, entryGarageTransaction.TenantId!.Value);
        await _garageTransactionUtilityAppService.UpdateEntryTransactionCheckouFromMobile(entryTransactionId);
        var garageTransaction = await _garageTransactionsAppService.CreateAsync(createdExitTransaction);
        invoice.Value.GarageTransactionExitId = garageTransaction.Id;
        return await _invoiceAppService.CreateAsync(invoice.Value);

    }

    ///<inheritdoc/>
    public async Task<Result<CreateUpdateInvoiceDto>> PreCheckout(int entryTransactionId)
    {
        var entryGarageTransaction = await _garageTransactionUtilityAppService.GetTransactionById(entryTransactionId);
        var createdExitGarageTransaction =await _garageTransactionUtilityAppService.InitiateExitGarageTransactionForMobileCheckout(entryGarageTransaction.PlateNumber, entryGarageTransaction.VehiclePicture, entryGarageTransaction.OrganizationID, entryGarageTransaction.GarageId, entryGarageTransaction.CameraId, entryGarageTransaction.TenantId!.Value);
        var organization = await _garageTransactionUtilityAppService.GetOrganization(createdExitGarageTransaction.OrganizationID);
        int totalHours = _calculationsEndPointAppService.CalculateTime(entryGarageTransaction.AccurateTime.Value, (DateTime)createdExitGarageTransaction.AccurateTime, organization.FreeMinutes);
        var customerPackageDto = await _customerPackagesAppService.GetOneActiveCustomerPackageByOrganizationIdAndPlateNumber(createdExitGarageTransaction.OrganizationID, createdExitGarageTransaction.PlateNumber, createdExitGarageTransaction.AccurateTime.Value);
        if (customerPackageDto == null)
        {
            decimal price =  _calculationsEndPointAppService.CalculateInvoiceCases(entryGarageTransaction.AccurateTime.Value, totalHours, createdExitGarageTransaction.AccurateTime.Value, organization);
            var result = await _garageTransactionUtilityAppService.InitiateInvoice(entryGarageTransaction.AccurateTime.Value, createdExitGarageTransaction, totalHours, price, false);
            return Result.Success(result);

        }
        createdExitGarageTransaction.CustomerCategory = customerPackageDto.CustomerCategory;
        Customer exsitCustomer = await _garageTransactionUtilityAppService.GetCustomer(customerPackageDto.CustomerId);
        var _customerInvoicePrice = _calculationsEndPointAppService.CalculationForRegisteredCustomer(createdExitGarageTransaction.AccurateTime.Value, entryGarageTransaction.AccurateTime.Value, organization, customerPackageDto, totalHours);
        CreateUpdateInvoiceDto invoice = await _garageTransactionUtilityAppService.InitiateInvoice(entryGarageTransaction.AccurateTime.Value, createdExitGarageTransaction, totalHours, _customerInvoicePrice, true, exsitCustomer);
        return Result.Success(invoice);

    
    }
    #endregion

}

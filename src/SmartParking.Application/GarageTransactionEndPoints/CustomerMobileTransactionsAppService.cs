using Microsoft.AspNetCore.Authorization;
using SmartParking.CalculationsEndpoint;
using SmartParking.CustomerPackages;
using SmartParking.Entities.Customers;
using SmartParking.Entities.GarageTransactions;
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
public class CustomerMobileTransactionsAppService : SmartParkingAppService, ICustomerMobileTransactionsAppService
{
    private readonly IInvoiceAppService _invoiceAppService;
    private readonly ICalculationsEndPointsAppService _calculationsEndPointAppService;
    private readonly IGarageTransactionUtilityAppService _garageTransactionUtilityAppService;
    private readonly IGarageTransactionsAppService _garageTransactionsAppService;
    private readonly ICustomerPackagesAppService _customerPackagesAppService;

    public CustomerMobileTransactionsAppService(IGarageTransactionUtilityAppService garageTransactionUtilityAppService, ICalculationsEndPointsAppService calculationsEndPointAppService, IInvoiceAppService invoiceAppService, IGarageTransactionsAppService garageTransactionsAppService, ICustomerPackagesAppService customerPackagesAppService)
    {
        _garageTransactionUtilityAppService = garageTransactionUtilityAppService;
        _calculationsEndPointAppService = calculationsEndPointAppService;
        _invoiceAppService = invoiceAppService;
        _garageTransactionsAppService = garageTransactionsAppService;
        _customerPackagesAppService = customerPackagesAppService;
    }

    #region public methods for customer transactions

    ///<inheritdoc/>
    public async Task<Result<CreateUpdateInvoiceDto>> PreCheckout(int entryTransactionId)
    {
        var customerId = await _garageTransactionUtilityAppService.GetCurrentCustomerId();
        var Customer = await _garageTransactionUtilityAppService.GetCustomer(customerId);
        var entryGarageTransaction = await _garageTransactionUtilityAppService.GetTransactionById(entryTransactionId);
        var createdExitGarageTransaction =await _garageTransactionUtilityAppService.InitiateExitGarageTransactionForMobileCheckout(entryGarageTransaction.PlateNumber,entryGarageTransaction.VehiclePicture,entryGarageTransaction.OrganizationID,entryGarageTransaction.GarageId,entryGarageTransaction.CameraId,entryGarageTransaction.TenantId!.Value);
        var invoice = await PreCheckoutFlow(entryGarageTransaction.AccurateTime.Value, createdExitGarageTransaction, Customer);
        return invoice;

    }
    ///<inheritdoc/>
    public async Task<Result<InvoiceDto>> Checkout(int entryTransactionId)
    {

        var entryGarageTransaction = await _garageTransactionUtilityAppService.GetTransactionById(entryTransactionId);
        var createdExitGarageTransaction = await _garageTransactionUtilityAppService.InitiateExitGarageTransactionForMobileCheckout(entryGarageTransaction.PlateNumber, entryGarageTransaction.VehiclePicture, entryGarageTransaction.OrganizationID, entryGarageTransaction.GarageId, entryGarageTransaction.CameraId, entryGarageTransaction.TenantId!.Value);
        var lstCheckout = entryGarageTransaction.CheckoutMobile.HasValue ? entryGarageTransaction.CheckoutMobile.Value : DateTime.MinValue;
        var _garageTransactionDto = ObjectMapper.Map<GarageTransaction, GarageTransactionDto>(entryGarageTransaction);
        if (!(_garageTransactionDto.IsIn && _garageTransactionDto.IsPending == false && _garageTransactionDto.CanCheckout == true))
        {
            throw new Exception("Transaction Already Checked-Out");
        }
        var invoice = await PreCheckout(entryTransactionId);
        await _garageTransactionUtilityAppService.UpdateEntryTransactionCheckouFromMobile(entryTransactionId);
        var savedExitgarageTransaction = await _garageTransactionsAppService.CreateAsync(createdExitGarageTransaction);
        invoice.Value.GarageTransactionExitId = savedExitgarageTransaction.Id;
        return await _invoiceAppService.CreateAsync(invoice);
    }
    #endregion
    #region private methods to get pre invoice
    /// <summary>
    /// Method used to get pre invoice
    /// </summary>
    /// <param name="entranceTime">Entrance timer</param>
    /// <param name="exitTransaction">Exit Time</param>
    /// <param name="customer">Customer</param>
    /// <returns>Pre Invoice</returns>
    private async Task<CreateUpdateInvoiceDto> PreCheckoutFlow(DateTime entranceTime, CreateUpdateGarageTransactionDto exitTransaction, Customer customer)
    {
        var organization = await _garageTransactionUtilityAppService.GetOrganization(exitTransaction.OrganizationID);
        int totalHours = _calculationsEndPointAppService.CalculateTime(entranceTime, exitTransaction.AccurateTime.Value, organization.FreeMinutes);
        if (totalHours == 0)
        {
            return await _garageTransactionUtilityAppService.InitiateInvoice(entranceTime, exitTransaction, totalHours, 0, false,customer);

        }
        var packageDto = await _customerPackagesAppService.GetOneActiveCustomerPackageByOrganizationIdAndPlateNumber(exitTransaction.OrganizationID, exitTransaction.PlateNumber, exitTransaction.AccurateTime.Value);
        var _price = _calculationsEndPointAppService.CalculationForRegisteredCustomer(entranceTime, exitTransaction.AccurateTime.Value, organization, packageDto, totalHours);
        CreateUpdateInvoiceDto newInvoice = await _garageTransactionUtilityAppService.InitiateInvoice(entranceTime, exitTransaction, totalHours, _price, true,customer);
        newInvoice.EntranceTime = entranceTime;
        return newInvoice;
    }
    #endregion

}

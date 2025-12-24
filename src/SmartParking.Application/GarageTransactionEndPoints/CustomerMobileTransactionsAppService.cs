using Microsoft.AspNetCore.Authorization;
using SmartParking.CalculationsEndpoint;
using SmartParking.CustomerPackages;
using SmartParking.CustomerPackages.CustomerPackageTransactions;
using SmartParking.Cutomers;
using SmartParking.GarageTransactions;
using SmartParking.Invoices;
using SmartParking.Results;
using SmartParking.Utilities;
using System;
using System.Threading.Tasks;

namespace SmartParking.GarageTransactionEndPoints;

[Authorize]
public class CustomerMobileTransactionsAppService : SmartParkingAppService, ICustomerMobileTransactionsAppService
{
    private readonly IInvoiceAppService _invoiceAppService;
    private readonly ICalculationsEndPointsAppService _calculationsEndPointAppService;
    private readonly IGarageTransactionUtilityAppService _garageTransactionUtilityAppService;
    private readonly IGarageTransactionsAppService _garageTransactionsAppService;
    private readonly ICustomerPackageHandlerAppService _customerPackagesHandlerAppService;

    public CustomerMobileTransactionsAppService(IGarageTransactionUtilityAppService garageTransactionUtilityAppService, ICalculationsEndPointsAppService calculationsEndPointAppService, IInvoiceAppService invoiceAppService, IGarageTransactionsAppService garageTransactionsAppService, ICustomerPackageHandlerAppService customerPackagesHandlerAppService)
    {
        _garageTransactionUtilityAppService = garageTransactionUtilityAppService;
        _calculationsEndPointAppService = calculationsEndPointAppService;
        _invoiceAppService = invoiceAppService;
        _garageTransactionsAppService = garageTransactionsAppService;
        _customerPackagesHandlerAppService = customerPackagesHandlerAppService;
    }

    #region public methods for customer transactions

    ///<inheritdoc/>
    public async Task<Result<CreateUpdateInvoiceDto>> PreCheckout(int entryTransactionId)
    {

        var customer = await _garageTransactionUtilityAppService.GetCurrentCustomer();
        var entryGarageTransaction = await _garageTransactionUtilityAppService.GetTransactionById(entryTransactionId);
        if (entryGarageTransaction.CanCheckout == false)
        {
                throw new Exception("You have to wait Grace Period");
        }
        var createdExitGarageTransaction =await _garageTransactionUtilityAppService.InitiateExitGarageTransactionForMobileCheckout(entryGarageTransaction);
        var invoice = await PreCheckoutFlow(entryGarageTransaction.AccurateTime.Value, createdExitGarageTransaction, customer);
        return invoice;

    }
    ///<inheritdoc/>
    public async Task<Result<InvoiceDto>> Checkout(int entryTransactionId)
    {

        var _entryGarageTransactionDto = await _garageTransactionUtilityAppService.GetTransactionById(entryTransactionId);
        var createdExitGarageTransaction = await _garageTransactionUtilityAppService.InitiateExitGarageTransactionForMobileCheckout(_entryGarageTransactionDto);
        if ( _entryGarageTransactionDto.CanCheckout == false)
        {
            if(_entryGarageTransactionDto.IsIn==false)
                throw new Exception("Transaction Already Checked-Out");
            else
                throw new Exception("You have to wait Grace Period");
        }
        var invoice = await PreCheckout(entryTransactionId);
        await _garageTransactionUtilityAppService.UpdateEntryTransactionCheckouFromMobile(entryTransactionId);
        var savedExitgarageTransaction = await _garageTransactionsAppService.CreateAsync(createdExitGarageTransaction);
        invoice.Value.GarageTransactionExitId = savedExitgarageTransaction.Id;
        invoice.Value.ExitTransactionImage = savedExitgarageTransaction.VehiclePicture;
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
    private async Task<CreateUpdateInvoiceDto> PreCheckoutFlow(DateTime entranceTime, CreateUpdateGarageTransactionDto exitTransaction, CustomerDto customer)
    {
        var organization = await _garageTransactionUtilityAppService.GetOrganization(exitTransaction.OrganizationID);
        int totalHours = _calculationsEndPointAppService.CalculateTime(entranceTime, exitTransaction.AccurateTime.Value, organization.FreeMinutes);
        if (totalHours == 0)
        {
            return await _garageTransactionUtilityAppService.InitiateInvoice(entranceTime, exitTransaction, totalHours, 0, false,customer);

        }
        var packageDto = await _customerPackagesHandlerAppService.GetOneActiveCustomerPackage(new GarageTransactionDto { OrganizationID = exitTransaction.OrganizationID, PlateNumber = exitTransaction.PlateNumber, TicketId = exitTransaction.TicketId, AccurateTime = exitTransaction.AccurateTime });
        var _price = _calculationsEndPointAppService.CalculationForRegisteredCustomer(exitTransaction.AccurateTime.Value,entranceTime, organization, packageDto, totalHours);
        CreateUpdateInvoiceDto newInvoice = await _garageTransactionUtilityAppService.InitiateInvoice(entranceTime, exitTransaction, totalHours, _price, packageDto is not null,customer);
        newInvoice.EntranceTime = entranceTime;
        return newInvoice;
    }
    #endregion

}

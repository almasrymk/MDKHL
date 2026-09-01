using Microsoft.AspNetCore.Authorization;
using SmartParking.CalculationsEndpoint;
using SmartParking.CustomerPackages.CustomerPackageTransactions;
using SmartParking.GarageTransactions;
using SmartParking.Invoices;
using SmartParking.OpeningGate;
using SmartParking.Organizations;
using SmartParking.Results;
using SmartParking.Utilities;
using System;
using System.Threading.Tasks;

namespace SmartParking.GarageTransactionEndPoints;

//[Authorize]
public class KeeperTransactionsAppService : SmartParkingAppService, IKeeperTransactionsAppService
{
    private readonly IInvoiceAppService _invoiceAppService;
    private readonly ICalculationsEndPointsAppService _calculationsEndPointAppService;
    private readonly IGarageTransactionUtilityAppService _garageTransactionUtilityAppService;
    private readonly IGarageTransactionsAppService _garageTransactionsAppService;
    private readonly ICustomerPackageHandlerAppService _customerPackagesHandlerAppService;
    private readonly IOrganizationsAppService _organizationAppService;
    private readonly IOpenGateFactoryService _openGateFactoryAppService;

    public KeeperTransactionsAppService(IInvoiceAppService invoiceAppService, ICalculationsEndPointsAppService calculationsEndPointAppService, IGarageTransactionUtilityAppService garageTransactionUtilityAppService, IGarageTransactionsAppService garageTransactionsAppService, ICustomerPackageHandlerAppService customerPackagesHandlerAppService, IOrganizationsAppService organizationAppService, IOpenGateFactoryService openGateAppService)
    {
        _invoiceAppService = invoiceAppService;
        _calculationsEndPointAppService = calculationsEndPointAppService;
        _garageTransactionUtilityAppService = garageTransactionUtilityAppService;
        _garageTransactionsAppService = garageTransactionsAppService;
        _customerPackagesHandlerAppService = customerPackagesHandlerAppService;
        _organizationAppService = organizationAppService;
        _openGateFactoryAppService = openGateAppService;
        _customerPackagesHandlerAppService = customerPackagesHandlerAppService;
    }


    #region public methods for keeper services

    ///<inheritdoc/>
    public async Task<Result<InvoiceDto>> Checkout(int entryTransactionId)
    {
        var entryGarageTransaction = await _garageTransactionUtilityAppService.GetTransactionById(entryTransactionId);
        var invoice = await PreCheckout(entryTransactionId);
        return await CheckoutProcess( entryGarageTransaction, invoice);
    }

    private async Task<Result<InvoiceDto>> CheckoutProcess(GarageTransactionDto entryGarageTransaction, CreateUpdateInvoiceDto invoice)
    {
        if (entryGarageTransaction.CanCheckout == false)
        {
            if (entryGarageTransaction.IsIn == false)
                throw new Exception("Transaction Already Checked-Out");
            else
                throw new Exception("You have to wait Grace Period");
        }
        var createdExitTransaction = await _garageTransactionUtilityAppService.InitiateExitGarageTransactionForMobileCheckout(entryGarageTransaction);
        // Maintain invariant: reset all prior CanCheckout=true for this vehicle before creating new stack top
        await _garageTransactionUtilityAppService.ResetCanCheckoutForVehicle(entryGarageTransaction.PlateNumber, entryGarageTransaction.TicketId);
        await _garageTransactionUtilityAppService.UpdateEntryTransactionCheckouFromMobile(entryGarageTransaction.Id);
        var garageTransaction = await _garageTransactionsAppService.CreateAsync(createdExitTransaction);
        invoice.GarageTransactionExitId = garageTransaction.Id;
        invoice.ExitTransactionImage = garageTransaction.VehiclePicture;
        return await _invoiceAppService.CreateAsync(invoice);
    }

    public async Task<Result<InvoiceDto>> Checkout(GarageTransactionDto entryGarageTransaction, CreateUpdateInvoiceDto PreInvoice)
    {
        return await CheckoutProcess(entryGarageTransaction, PreInvoice);
    }

    public async Task<bool> OpenGate(OpenGateDto openGateDto)
    {
        var _organization = await _organizationAppService.GetOrganizationForCurrentTenatAsync();
        var _openGateService =await _openGateFactoryAppService.GetOpeningGateService(_organization);
        return await _openGateService.OpenGateAsync(openGateDto);
    }

    ///<inheritdoc/>
    public async Task<Result<CreateUpdateInvoiceDto>> PreCheckout(int entryTransactionId)
    {
        var entryGarageTransaction = await _garageTransactionUtilityAppService.GetTransactionById(entryTransactionId);
        if (entryGarageTransaction.CanCheckout == false)
        {
            throw new Exception("You have to wait Grace Period");
        }
        var createdExitGarageTransaction = await _garageTransactionUtilityAppService.InitiateExitGarageTransactionForMobileCheckout(entryGarageTransaction);
        var organization = await _garageTransactionUtilityAppService.GetOrganization(createdExitGarageTransaction.OrganizationID);
        int totalHours = _calculationsEndPointAppService.CalculateTime(entryGarageTransaction.AccurateTime.Value, (DateTime)createdExitGarageTransaction.AccurateTime, organization.FreeMinutes);
        var customerPackageDto = await _customerPackagesHandlerAppService.GetOneActiveCustomerPackage(new GarageTransactionDto { OrganizationID = createdExitGarageTransaction.OrganizationID, AccurateTime = createdExitGarageTransaction.AccurateTime, PlateNumber = createdExitGarageTransaction.PlateNumber, TicketId = createdExitGarageTransaction.TicketId });
        if (customerPackageDto == null)
        {
            decimal price =  _calculationsEndPointAppService.CalculateInvoiceCases(entryGarageTransaction.AccurateTime.Value, totalHours, createdExitGarageTransaction.AccurateTime.Value, organization);
            var result = await _garageTransactionUtilityAppService.InitiateInvoice(entryGarageTransaction.AccurateTime.Value, createdExitGarageTransaction, totalHours, price, false);
            return Result.Success(result);

        }
        createdExitGarageTransaction.CustomerCategory = customerPackageDto.CustomerCategory;
        var exsitCustomer = await _garageTransactionUtilityAppService.GetCustomer(customerPackageDto.CustomerId);
        var _customerInvoicePrice = _calculationsEndPointAppService.CalculationForRegisteredCustomer(createdExitGarageTransaction.AccurateTime.Value, entryGarageTransaction.AccurateTime.Value, organization, customerPackageDto, totalHours);
        CreateUpdateInvoiceDto invoice = await _garageTransactionUtilityAppService.InitiateInvoice(entryGarageTransaction.AccurateTime.Value, createdExitGarageTransaction, totalHours, _customerInvoicePrice, true, exsitCustomer);
        return Result.Success(invoice);

    
    }
    #endregion

}

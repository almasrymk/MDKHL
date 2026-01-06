using Microsoft.AspNetCore.Authorization;
using SmartParking.CalculationsEndpoint;
using SmartParking.CustomerPackages;
using SmartParking.CustomerPackages.CustomerPackageTransactions;
using SmartParking.Cutomers;
using SmartParking.Entities.Vehicles;
using SmartParking.GarageTransactions;
using SmartParking.Invoices;
using SmartParking.Results;
using SmartParking.Utilities;
using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.GarageTransactionEndPoints;

[Authorize]
public class CustomerMobileTransactionsAppService : SmartParkingAppService, ICustomerMobileTransactionsAppService
{
    private readonly IInvoiceAppService _invoiceAppService;
    private readonly ICalculationsEndPointsAppService _calculationsEndPointAppService;
    private readonly IGarageTransactionUtilityAppService _garageTransactionUtilityAppService;
    private readonly IGarageTransactionsAppService _garageTransactionsAppService;
    private readonly ICustomerPackageHandlerAppService _customerPackagesHandlerAppService;
    private readonly IRepository<Vehicle, int> _vehicleRepository;
    private readonly IDataFilter _dataFilter;
    public CustomerMobileTransactionsAppService(IGarageTransactionUtilityAppService garageTransactionUtilityAppService, 
        ICalculationsEndPointsAppService calculationsEndPointAppService, 
        IInvoiceAppService invoiceAppService, 
        IGarageTransactionsAppService garageTransactionsAppService, 
        ICustomerPackageHandlerAppService customerPackagesHandlerAppService,
        IRepository<Vehicle, int> vehicleRepository,
        IDataFilter dataFilter)
    {
        _garageTransactionUtilityAppService = garageTransactionUtilityAppService;
        _calculationsEndPointAppService = calculationsEndPointAppService;
        _invoiceAppService = invoiceAppService;
        _garageTransactionsAppService = garageTransactionsAppService;
        _customerPackagesHandlerAppService = customerPackagesHandlerAppService;
        _vehicleRepository = vehicleRepository;
        _dataFilter = dataFilter;
    }

    #region public methods for customer transactions

    ///<inheritdoc/>
    public async Task<Result<CreateUpdateInvoiceDto>> PreCheckout(int entryTransactionId)
    {

        var customer = await _garageTransactionUtilityAppService.GetCurrentCustomer();
        var entryGarageTransaction = await _garageTransactionUtilityAppService.GetTransactionById(entryTransactionId);
        Vehicle? vehicle = null;
        using (_dataFilter.Disable<IMultiTenant>())
        {
            vehicle = await _vehicleRepository.FirstOrDefaultAsync(x => x.EnPlateNumber == entryGarageTransaction.PlateNumber);

        }
        if (entryGarageTransaction.CanCheckout == false)
        {
                throw new Exception("You have to wait Grace Period");
        }
        var createdExitGarageTransaction =await _garageTransactionUtilityAppService.InitiateExitGarageTransactionForMobileCheckout(entryGarageTransaction);
        var invoice = await PreCheckoutFlow(entryGarageTransaction.AccurateTime.Value, createdExitGarageTransaction, customer);
        invoice.LicenseTypeImage = vehicle is null ? "" : vehicle.LicenceType.Image;
        invoice.VehicleModelImage = vehicle is null ? "" : vehicle.VehicleModel.Image;
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
        var data =  await _invoiceAppService.CreateAsync(invoice);
        data.LicenseTypeImage = invoice.Value.LicenseTypeImage;
        data.VehicleModelImage = invoice.Value.VehicleModelImage;
        return data;
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

using SmartParking.Cameras;
using SmartParking.Cutomers;
using SmartParking.Entities.Cameras.Watchers;
using SmartParking.Entities.Customers;
using SmartParking.Entities.GarageTransactions;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.Garages;
using SmartParking.GarageTransactions;
using SmartParking.Invoices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartParking.Utilities;

public interface IGarageTransactionUtilityAppService : IApplicationService
{
    Task<Camera> GetCameraByDeviceIdAsync(string deviceId);

    Task<Camera> GetCameraByTypeBelongsToGarageAsync(int garageId, WatcherType cameraType);

    Task<Garage> GetGarageByIdAsync(int garageId);
    Task<bool> FreeSlotsOfGarageDependsOnCategoryAsync(int garageId, bool haseFreeSlots ,  CustomerCategoryStatus category);
    Task<bool> ReserverSlotsOfGarageDependsOnCategoryAsync(int garageId , bool hasFreeSlots , CustomerCategoryStatus customer);
    Task<string> CheckPlateNumberBelongsToCustomerToGetHisIdAsync(string plateNumber);

    Task<GarageTransactionDto?> FindEntranceTransaction(string? PlateNumber, string? TicketId, bool DispenserTransaction, bool CameraTransaction, int organizationId);
    Task<string> FindEntranceImage(string? plateNumber, string ticketId,int OrganiztionId);
    Task<GarageTransaction> CheckIfCheckoutFromMobile(string plateNumber);
    Task<GarageTransaction> GetTransactionById(int garagetransactionId);
    Task CloseEntryTransaction(string? plateNumber , string ticketId);
    Task CloseCheckoutMobileTransaction(string? plateNumber);
    Task UpdateEntryTransactionCheckouFromMobile(int entryGarageTransactionId);
    Task<Guid> GetCurrentCustomerId();
    Task<CustomerDto> GetCustomer(Guid customerId);
    Task<Organization> GetOrganization(int OrganizationID);
    Task<CreateUpdateGarageTransactionDto> InitiateExitGarageTransactionForMobileCheckout(string PlateNumber, string EntryPicture, int OrganizationId, int GarageId, int CameraId, Guid TenantId);
    Task<CreateUpdateInvoiceDto> InitiateInvoice(DateTime entranceTime, CreateUpdateGarageTransactionDto exitTransaction, int totalHours, decimal price, bool isPackage, CustomerDto customer = null);


}

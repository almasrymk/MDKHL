using SmartParking.Cameras;
using SmartParking.Cutomers;
using SmartParking.Entities.GarageTransactions;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.GarageTransactions;
using SmartParking.Invoices;
using SmartParking.Organizations;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartParking.Utilities;

public interface IGarageTransactionUtilityAppService : IApplicationService
{
    Task<CameraForGarageTransactionDto> GetCameraByDeviceIdAsync(string deviceId);
    Task<Garage> GetGarageByIdAsync(int garageId);
    Task<bool> FreeSlotsOfGarageDependsOnCategoryAsync(int garageId, bool haseFreeSlots ,  CustomerCategoryStatus category);
    Task<bool> ReserverSlotsOfGarageDependsOnCategoryAsync(int garageId , bool hasFreeSlots , CustomerCategoryStatus customer);
    Task<string> CheckPlateNumberBelongsToCustomerToGetHisIdAsync(string plateNumber);
    Task<GarageTransactionDto?> FindEntranceTransaction(string? PlateNumber, string? TicketId, bool DispenserTransaction, bool CameraTransaction, int organizationId , bool hasActivePackage);
    Task<string> FindEntranceImage(string? plateNumber, string ticketId,int OrganiztionId);
    Task<GarageTransactionDto> CheckIfCheckoutFromMobile(string plateNumber);
    Task<GarageTransaction> GetTransactionById(int garagetransactionId);
    Task CloseEntryTransaction(string? plateNumber , string ticketId , int entryTransactionId);
    Task CloseCheckoutMobileTransaction(string? plateNumber , int entryTransactionId);
    Task UpdateEntryTransactionCheckouFromMobile(int entryGarageTransactionId);
    Task<Guid> GetCurrentCustomerId();
    Task<CustomerDto> GetCustomer(Guid customerId);
    Task<TransactionOrganizationDto> GetOrganization(int OrganizationID);
    Task<CreateUpdateGarageTransactionDto> InitiateExitGarageTransactionForMobileCheckout(string PlateNumber, string EntryPicture, int OrganizationId, int GarageId, int CameraId, Guid TenantId);
    Task<CreateUpdateGarageTransactionDto> InitiateExitGarageTransactionForMobileCheckout(string PlateNumber, string EntryPicture, int OrganizationId, int GarageId, string LaneNO, string TicketId ,  Guid TenantId);
    Task<CreateUpdateInvoiceDto> InitiateInvoice(DateTime entranceTime, CreateUpdateGarageTransactionDto exitTransaction, int totalHours, decimal price, bool isPackage, CustomerDto customer = null);
}

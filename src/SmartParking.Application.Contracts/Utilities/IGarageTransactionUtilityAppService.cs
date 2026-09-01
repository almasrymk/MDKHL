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
    Task<Guid?> CheckPlateNumberBelongsToCustomerToGetHisIdAsync(string plateNumber);
    Task<GarageTransactionDto?> FindEntranceTransaction(string? PlateNumber, string? TicketId, bool DispenserTransaction, bool CameraTransaction, int organizationId , bool hasActivePackage);
    Task<string> FindEntranceImage(string? plateNumber, string ticketId,int OrganiztionId);
    Task<GarageTransactionDto> GetTransactionById(int garagetransactionId);
    Task CloseEntryTransaction(string? plateNumber , string ticketId);
    Task CloseCheckoutMobileTransaction(string? plateNumber);
    Task UpdateEntryTransactionCheckouFromMobile(int entryGarageTransactionId);
    Task<CustomerDto> GetCurrentCustomer();
    Task<CustomerDto> GetCustomer(Guid customerId);
    Task<TransactionOrganizationDto> GetOrganization(string OrganizationCode);
    Task<TransactionOrganizationDto> GetOrganization(int OrganizationId);
    Task<CreateUpdateGarageTransactionDto> InitiateExitGarageTransactionForMobileCheckout(GarageTransactionDto garageTransactionDto);
    Task<CreateUpdateInvoiceDto> InitiateInvoice(DateTime entranceTime, CreateUpdateGarageTransactionDto exitTransaction, int totalHours, decimal price, bool isPackage, CustomerDto customer = null);
    /// <summary>Resets CanCheckout=false for all transactions of a vehicle. Enforces global invariant: at most one CanCheckout=true per vehicle.</summary>
    Task ResetCanCheckoutForVehicle(string plateNumber, string? ticketId);
}

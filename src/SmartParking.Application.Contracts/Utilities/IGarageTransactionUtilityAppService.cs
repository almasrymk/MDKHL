using SmartParking.Cameras;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Customers;
using SmartParking.Entities.GarageTransactions;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.Garages;
using SmartParking.GarageTransactions;
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

    Task<CameraDto> GetCameraByTypeBelongsToGarageAsync(int garageId, CameraType cameraType);

    Task<Garage> GetGarageByIdAsync(int garageId);
    Task<bool> FreeSlotsOfGarageDependsOnCategoryAsync(int garageId, CustomerCategoryStatus category);
    Task<bool> ReserverSlotsOfGarageDependsOnCategoryAsync(int garageId , CustomerCategoryStatus customer);
    Task<string> CheckPlateNumberBelongsToCustomerToGetHisIdAsync(string plateNumber);

    Task<GarageTransactionDto?> FindEntranceTime(string? plateNumber);
    Task<string> FindEntranceImage(string? plateNumber);
    Task<GarageTransaction> CheckIfCheckoutFromMobile(string plateNumber);
    Task<GarageTransaction> GetTransactionById(int garagetransactionId);
    Task CloseEntryTransaction(string? plateNumber);
    Task CloseCheckoutMobileTransaction(string? plateNumber);
    Task UpdateEntryTransactionCheckouFromMobile(GarageTransaction entryGarageTransaction);
    Task<Guid> GetCurrentCustomerId();
    Task<Customer> GetCustomer(Guid customerId);
    Task<Organization> GetOrganization(int OrganizationID);


}

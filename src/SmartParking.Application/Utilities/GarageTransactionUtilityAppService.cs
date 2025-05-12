using SmartParking.Cameras;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.Localization;
using System;
using Volo.Abp.Domain.Repositories;
using System.Threading.Tasks;
using System.Diagnostics.Metrics;
using SmartParking.Entities.Vehicles;
using Microsoft.Extensions.Logging;
using SmartParking.GarageTransactionEndPoints;
using SmartParking.Entities.GarageTransactions;
using SmartParking.GarageTransactions;
using Volo.Abp.MultiTenancy;
using System.Linq;
using Volo.Abp.Data;
using Microsoft.EntityFrameworkCore;
using SmartParking.Entities.Customers;


namespace SmartParking.Utilities;

public class GarageTransactionUtilityAppService : SmartParkingAppService, IGarageTransactionUtilityAppService
{
    private readonly ILogger<GarageTransactionUtilityAppService> _logger;
    private readonly IRepository<Camera, int> _cameraRepository;
    private readonly IRepository<Garage, int> _garageRepository;
    private readonly IRepository<Vehicle, int> _vehicleRepository;
    private readonly IRepository<GarageTransaction, int> _garageTransactionRepoistory;
    private readonly IRepository<Customer, Guid> _customerRepoistory;
    private readonly IRepository<Organization, int> _organizationRepoistory;
    private readonly IDataFilter _dataFilter;




    public GarageTransactionUtilityAppService(
        ILogger<GarageTransactionUtilityAppService> logger,
        IRepository<Camera, int> cameraRepository,
        IRepository<Garage, int> garageRepository,
        IRepository<Vehicle, int> vehicleRepository,
        IRepository<GarageTransaction, int> garageTransactionRepoistory,
        IDataFilter dataFilter,
        IRepository<Customer, Guid> customerRepoistory,
        IRepository<Organization, int> organizationRepoistory)
    {
        _cameraRepository = cameraRepository;
        _garageRepository = garageRepository;
        _vehicleRepository = vehicleRepository;
        _logger = logger;
        _garageTransactionRepoistory = garageTransactionRepoistory;
        _dataFilter = dataFilter;
        _customerRepoistory = customerRepoistory;
        _organizationRepoistory = organizationRepoistory;
    }

    public async Task<GarageTransaction> CheckIfCheckoutFromMobile(string plateNumber)
    {
         using (DataFilter.Disable<IMultiTenant>())
         {

            var query = await _garageTransactionRepoistory.GetQueryableAsync();


            var latestTransaction = query
                    .Where(gt => gt.PlateNumber == plateNumber && gt.CameraType == CameraType.Exit && gt.IsIn == true)
                    .OrderByDescending(gt => gt.AccurateTime)
                    .FirstOrDefault();

            return latestTransaction;
         }
    }

    public async Task<string> CheckPlateNumberBelongsToCustomerToGetHisIdAsync(string plateNumber)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {

            var Vehicle = await _vehicleRepository.FirstOrDefaultAsync(x => x.ArPlateNumber == plateNumber || x.EnPlateNumber == plateNumber);
            if (Vehicle == null)
            {
                _logger.LogError($"{DateTime.Now} ====>  Vehicle Not Found ");
                return null;
            }
            var CustomerId = Vehicle.CustomerId;
            return CustomerId.ToString();
        }
    }

    public async Task CloseCheckoutMobileTransaction(string? plateNumber)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var query = await _garageTransactionRepoistory.GetQueryableAsync();
            query.Where(gt => gt.CameraType == CameraType.Exit);
            await SaveClosedEntryTransaction(plateNumber, query);
        }
    }

    public async Task CloseEntryTransaction(string? plateNumber)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var query = await _garageTransactionRepoistory.GetQueryableAsync();
            await SaveClosedEntryTransaction(plateNumber, query);
        }
    }
    private async Task SaveClosedEntryTransaction(string? plateNumber, IQueryable<GarageTransaction> query)
    {
        var latestTransactions = query
                .Where(gt => gt.PlateNumber == plateNumber && gt.IsIn == true)
                .OrderByDescending(gt => gt.AccurateTime)
                .ToList();
        latestTransactions.ForEach(gt => {
            gt.IsIn = false;
            gt.IsMapped = true;
        });
        await CurrentUnitOfWork.SaveChangesAsync();
    }
    public async Task<string> FindEntranceImage(string? plateNumber)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {

            var query = await _garageTransactionRepoistory.GetQueryableAsync();
            var latestTransaction = query
                    .Where(gt => gt.PlateNumber == plateNumber && gt.CameraType == CameraType.Entry && gt.IsIn == true)
                    .OrderByDescending(gt => gt.AccurateTime) 
                    .FirstOrDefault();
            return latestTransaction?.VehiclePicture;
        } 
    }

    public async Task<GarageTransactionDto?> FindEntranceTime(string? plateNumber)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var query = await _garageTransactionRepoistory.GetQueryableAsync();
            var latestTransaction = query
                    .Where(gt => gt.PlateNumber == plateNumber && gt.CameraType == CameraType.Entry && gt.IsIn == true)
                    .OrderByDescending(gt => gt.AccurateTime)
                    .FirstOrDefault();
            return ObjectMapper.Map<GarageTransaction, GarageTransactionDto>(latestTransaction);
        }
    }

    public async Task<bool> FreeSlotsOfGarageDependsOnCategoryAsync(int garageId, CustomerCategoryStatus category)
    {
        var garage = await GetGarageByIdAsync(garageId);
        bool flag = false;
        if (category == CustomerCategoryStatus.Customer )
        {
            garage.CurrnetCapacityForCustomers = Math.Max(0, --garage.CurrnetCapacityForCustomers);
            flag = true;

        }
        else if (category == CustomerCategoryStatus.Employee)
        {
            garage.CurrnetCapacityEmployees = Math.Max(0, --garage.CurrnetCapacityEmployees);
            flag = true;
        }
        else
        {
            garage.CurrnetCapacityForFree = Math.Max(0, --garage.CurrnetCapacityForFree);
            flag = true;
        }
        garage.CurrnetCapicity = Math.Max(0, --garage.CurrnetCapicity);
        await CurrentUnitOfWork.SaveChangesAsync();
        return flag;
    }

    public async Task<Camera> GetCameraByDeviceIdAsync(string deviceId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var cameraquery = await _cameraRepository.GetQueryableAsync();
            var camera=await cameraquery.AsNoTracking().FirstOrDefaultAsync(cam => cam.DeviceId == deviceId);
            if (camera == null)
            {
                throw new Exception(LocalizationHelper.GetLocalizedString("Camera") + " " + LocalizationHelper.GetLocalizedString("NotFoundValidation"));
            }
            return camera;
        }
    }


    public async Task<CameraDto> GetCameraByTypeBelongsToGarageAsync(int garageId, CameraType cameraType)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var camera = await _cameraRepository.FirstOrDefaultAsync(cam => cam.GarageId == garageId && cam.CameraType == cameraType);
            if (camera == null)
            {
                throw new Exception(LocalizationHelper.GetLocalizedString("Camera") + " " + LocalizationHelper.GetLocalizedString("NotFoundValidation"));
            }
            return ObjectMapper.Map<Camera, CameraDto>(camera);
        }
    }

    public async Task<Garage> GetGarageByIdAsync(int garageId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var garage = await _garageRepository.FirstOrDefaultAsync(x => x.Id == garageId && x.Status == Status.Active);
            if (garage == null)
            {
                throw new Exception(LocalizationHelper.GetLocalizedString("Garage") + " " + LocalizationHelper.GetLocalizedString("NotFoundValidation"));
            }
            return garage;
        }

    }

    public async Task<GarageTransaction> GetTransactionById(int garagetransactionId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {

            var query = await _garageTransactionRepoistory.GetQueryableAsync();
            query = query.Where(transaction => transaction.Id == garagetransactionId);
            var transaction = await AsyncExecuter.FirstOrDefaultAsync(query);
            return transaction;
        }
    }

    public async Task<bool> ReserverSlotsOfGarageDependsOnCategoryAsync(int garageId, CustomerCategoryStatus customer)
    {
        var garage = await GetGarageByIdAsync(garageId);
        bool flag = false;
        switch (customer)
        {
            case CustomerCategoryStatus.Customer:
                if (garage.CurrnetCapacityForCustomers + 1 <= garage.MaxSlotsForCustomers)
                    garage.CurrnetCapacityForCustomers++;
                break;

            case CustomerCategoryStatus.Employee:
                if (garage.CurrnetCapacityEmployees + 1 <= garage.MaxSlotsForEmployess)
                    garage.CurrnetCapacityEmployees++;
                break;

            case CustomerCategoryStatus.Visitor:
                if (garage.CurrnetCapacityForFree + 1 <= garage.MaxSlotsForFree)
                    garage.CurrnetCapacityForFree++;
                break;

            default:
                if (garage.CurrnetCapacityForCustomers + 1 <= garage.MaxSlotsForCustomers)
                    garage.CurrnetCapacityForCustomers++;
                break;

        }
        if (garage.CurrnetCapicity + 1 <= garage.MaxCapacity)
        {
            garage.CurrnetCapicity++;
            flag = true;
        }
        return await Task.FromResult(flag);
    }

    public async Task UpdateEntryTransactionCheckouFromMobile(GarageTransaction entryGarageTransaction)
    {
        entryGarageTransaction.CheckoutMobile = DateTime.Now;
        entryGarageTransaction.IsPending = true;
        entryGarageTransaction.CanCheckout = false;
        await _garageTransactionRepoistory.UpdateAsync(entryGarageTransaction, true);

    }

    public async Task<Guid> GetCurrentCustomerId()
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var customer = await _customerRepoistory.GetQueryableAsync();
            var customerId = customer.Where(s => s.IdentityUserId == CurrentUser.Id).Select(s => s.Id).FirstOrDefault();
            return customerId;
        }
    }
    public async Task<Customer> GetCustomer(Guid customerId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _customerRepoistory.GetQueryableAsync();
            query = query.Include(x => x.IdentityUser);
            var customer = await query.FirstOrDefaultAsync(x => x.Id == customerId);
            return customer;
        }
    }
    public async Task<Organization> GetOrganization(int OrganizationID)
    {

        using (DataFilter.Disable<IMultiTenant>())
        {
            var organization = await _organizationRepoistory.FirstOrDefaultAsync(x => x.Id == OrganizationID);
            return organization;
        }

    }

}

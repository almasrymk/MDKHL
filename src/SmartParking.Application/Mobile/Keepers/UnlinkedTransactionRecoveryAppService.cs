using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartParking.CustomerPackages.CustomerPackageTransactions;
using SmartParking.Entities.Cameras.Watchers;
using SmartParking.Entities.GarageTransactions;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.GarageTransactions;
using SmartParking.Mobile.Keepers.Dtos;
using SmartParking.OpeningGate;
using SmartParking.Organizations;
using SmartParking.Results;
using SmartParking.Utilities;
using SmartParking.Validations;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace SmartParking.Mobile.Keepers;

[RemoteService(isEnabled: false)]
public class UnlinkedTransactionRecoveryAppService : SmartParkingAppService, IUnlinkedTransactionRecoveryAppService
{
    private readonly IRepository<UnLinkedTransaction, int> _unlinkedTransactionRepository;
    private readonly IRepository<Watcher, int> _watcherRepository;
    private readonly IRepository<Organization, int> _organizationRepository;
    private readonly IRepository<GarageTransaction, int> _garageTransactionRepository;
    private readonly IGarageTransactionsAppService _garageTransactionsAppService;
    private readonly ICustomerPackageHandlerAppService _customerPackageHandlerAppService;
    private readonly IOpenGateFactoryService _openGateFactoryAppService;
    private readonly IOrganizationsAppService _organizationAppService;
    private readonly IGarageTransactionUtilityAppService _garageTransactionUtilityAppService;

    public UnlinkedTransactionRecoveryAppService(
        IRepository<UnLinkedTransaction, int> unlinkedTransactionRepository,
        IRepository<Watcher, int> watcherRepository,
        IRepository<Organization, int> organizationRepository,
        IRepository<GarageTransaction, int> garageTransactionRepository,
        IGarageTransactionsAppService garageTransactionsAppService,
        ICustomerPackageHandlerAppService customerPackageHandlerAppService,
        IOpenGateFactoryService openGateFactoryAppService,
        IOrganizationsAppService organizationAppService,
        IGarageTransactionUtilityAppService garageTransactionUtilityAppService)
    {
        _unlinkedTransactionRepository = unlinkedTransactionRepository;
        _watcherRepository = watcherRepository;
        _organizationRepository = organizationRepository;
        _garageTransactionRepository = garageTransactionRepository;
        _garageTransactionsAppService = garageTransactionsAppService;
        _customerPackageHandlerAppService = customerPackageHandlerAppService;
        _openGateFactoryAppService = openGateFactoryAppService;
        _organizationAppService = organizationAppService;
        _garageTransactionUtilityAppService = garageTransactionUtilityAppService;
    }

    /// <inheritdoc/>
    public async Task<Result<UnlinkedTransactionViewDto>> GetLatestUnlinkedTransactionByWatcherCodeAsync(int organizationId, string watcherCode)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var org = await _organizationRepository.FirstOrDefaultAsync(o => o.Id == organizationId);
            if (org?.Settings?.UnlinkedEntry != true)
            {
                return Result<UnlinkedTransactionViewDto>.Invalid(new ValidationError
                {
                    ErrorMessage = "Unlinked entry recovery feature is not enabled for this organization."
                });
            }

            var watcher = await _watcherRepository.FirstOrDefaultAsync(w => w.DeviceId == watcherCode && w.OrganizationId == organizationId);
            if (watcher == null)
            {
                return Result<UnlinkedTransactionViewDto>.Success(new UnlinkedTransactionViewDto
                {
                    NoTransaction = true,
                    IsDispenserTransaction = org.DispenserTransaction,
                    IsCameraTransaction = org.CameraTransaction
                });
            }

            // Exclude Exit Watchers from Entry recovery
            if (watcher.CameraType == WatcherType.Exit)
            {
                return Result<UnlinkedTransactionViewDto>.Invalid(new ValidationError
                {
                    ErrorMessage = "Exit watcher transactions cannot be recovered as entry."
                });
            }

            var query = await _unlinkedTransactionRepository.GetQueryableAsync();
            var unlinked = await query
                .Where(u => u.DeviceId == watcherCode && u.IsDeleted == false && u.CreationTime >= DateTime.Now.AddMinutes(-10))
                .OrderByDescending(u => u.CreationTime)
                .FirstOrDefaultAsync();

            if (unlinked == null)
            {
                return Result<UnlinkedTransactionViewDto>.Success(new UnlinkedTransactionViewDto
                {
                    NoTransaction = true,
                    IsDispenserTransaction = org.DispenserTransaction,
                    IsCameraTransaction = org.CameraTransaction,
                    OrganizationId = watcher.OrganizationId,
                    GarageId = watcher.GarageId
                });
            }

            var dto = ObjectMapper.Map<UnLinkedTransaction, UnlinkedTransactionViewDto>(unlinked);
            dto.OrganizationId = watcher.OrganizationId;
            dto.GarageId = watcher.GarageId;
            dto.IsDispenserTransaction = org.DispenserTransaction;
            dto.IsCameraTransaction = org.CameraTransaction;
            dto.NoTransaction = false;

            return Result<UnlinkedTransactionViewDto>.Success(dto);
        }
    }

    /// <inheritdoc/>
    [UnitOfWork]
    public async Task<Result<GarageTransactionDto>> SendUnlinkedTransactionAsEntryAsync(SendUnlinkedAsEntryDto input)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var org = await _organizationRepository.FirstOrDefaultAsync(o => o.Id == input.OrganizationId);
            if (org?.Settings?.UnlinkedEntry != true)
            {
                return Result<GarageTransactionDto>.Invalid(new ValidationError
                {
                    ErrorMessage = "Unlinked entry recovery feature is not enabled for this organization."
                });
            }

            if (input.UnlinkedTransactionId <= 0)
            {
                return Result<GarageTransactionDto>.Invalid(new ValidationError
                {
                    ErrorMessage = "Transaction not found or has already been processed."
                });
            }

            var unlinked = await _unlinkedTransactionRepository.FirstOrDefaultAsync(u => u.Id == input.UnlinkedTransactionId && u.IsDeleted == false);
            if (unlinked == null)
            {
                return Result<GarageTransactionDto>.Invalid(new ValidationError
                {
                    ErrorMessage = "Transaction not found or has already been processed."
                });
            }

            var watcher = await _watcherRepository.FirstOrDefaultAsync(w => w.DeviceId == input.WatcherCode && w.OrganizationId == input.OrganizationId);
            if (watcher == null)
            {
                return Result<GarageTransactionDto>.Invalid(new ValidationError
                {
                    ErrorMessage = "Device not found under current organization."
                });
            }

            // Exclude Exit Watchers from Entry recovery
            if (watcher.CameraType == WatcherType.Exit)
            {
                return Result<GarageTransactionDto>.Invalid(new ValidationError
                {
                    ErrorMessage = "Cannot create entry transaction from an exit watcher."
                });
            }

            string? plate = null;
            string? ticket = null;

            if (org.DispenserTransaction)
            {
                ticket = (input.TicketNumber ?? "").Trim();
                if (string.IsNullOrEmpty(ticket))
                {
                    return Result<GarageTransactionDto>.Invalid(new ValidationError
                    {
                        ErrorMessage = "Ticket Number is required for dispenser transactions."
                    });
                }

                if (!VehicleValidation.IsValidTicketNumber(ticket))
                {
                    return Result<GarageTransactionDto>.Invalid(new ValidationError
                    {
                        ErrorMessage = "Invalid ticket number."
                    });
                }

                // Plate is optional for dispenser transactions
                string rawPlate = (input.CorrectedPlateNumber ?? unlinked.PlateNumber ?? "").Trim().Replace(" ", "");
                if (!string.IsNullOrEmpty(rawPlate))
                {
                    if (!VehicleValidation.IsValidSaudiPlate(rawPlate))
                    {
                        return Result<GarageTransactionDto>.Invalid(new ValidationError
                        {
                            ErrorMessage = "Invalid Saudi plate number."
                        });
                    }

                    plate = rawPlate;
                    var existingActivePlate = await _garageTransactionRepository.FirstOrDefaultAsync(
                        gt => gt.PlateNumber == plate && gt.OrganizationID == watcher.OrganizationId && gt.IsIn == true && gt.IsMapped == false);
                    if (existingActivePlate != null)
                    {
                        return Result<GarageTransactionDto>.Invalid(new ValidationError
                        {
                            ErrorMessage = $"An open entry transaction already exists for plate {plate}."
                        });
                    }
                }

                var existingActiveTicket = await _garageTransactionRepository.FirstOrDefaultAsync(
                    gt => gt.TicketId == ticket && gt.OrganizationID == watcher.OrganizationId && gt.IsIn == true && gt.IsMapped == false);
                if (existingActiveTicket != null)
                {
                    return Result<GarageTransactionDto>.Invalid(new ValidationError
                    {
                        ErrorMessage = $"An open entry transaction already exists for ticket {ticket}."
                    });
                }
            }
            else
            {
                // Camera transaction: Plate is mandatory
                plate = (input.CorrectedPlateNumber ?? unlinked.PlateNumber ?? "").Trim().Replace(" ", "");
                if (string.IsNullOrEmpty(plate))
                {
                    return Result<GarageTransactionDto>.Invalid(new ValidationError
                    {
                        ErrorMessage = "Plate number is required."
                    });
                }

                if (!VehicleValidation.IsValidSaudiPlate(plate))
                {
                    return Result<GarageTransactionDto>.Invalid(new ValidationError
                    {
                        ErrorMessage = "Invalid Saudi plate number."
                    });
                }

                var existingActive = await _garageTransactionRepository.FirstOrDefaultAsync(
                    gt => gt.PlateNumber == plate && gt.OrganizationID == watcher.OrganizationId && gt.IsIn == true && gt.IsMapped == false);
                if (existingActive != null)
                {
                    return Result<GarageTransactionDto>.Invalid(new ValidationError
                    {
                        ErrorMessage = $"An open entry transaction already exists for plate {plate}."
                    });
                }
            }

            var category = CustomerCategoryStatus.Customer;
            if (!string.IsNullOrEmpty(plate))
            {
                var packageDto = await _customerPackageHandlerAppService.GetOneActiveCustomerPackage(new GarageTransactionDto
                {
                    OrganizationID = watcher.OrganizationId,
                    PlateNumber = plate,
                    AccurateTime = unlinked.AccurateTime ?? unlinked.SnapTime ?? DateTime.Now
                });
                category = packageDto?.CustomerCategory ?? CustomerCategoryStatus.Customer;
            }

            var entryDto = new CreateUpdateGarageTransactionDto
            {
                PlateNumber = plate,
                TicketId = ticket,
                CameraId = watcher is Camera ? watcher.Id : (int?)null,
                DispenserId = watcher is Dispenser ? watcher.Id : (int?)null,
                OrganizationID = watcher.OrganizationId,
                GarageId = watcher.GarageId,
                TenantId = watcher.TenantId,
                CameraType = WatcherType.Entry,
                IsIn = true,
                IsMapped = false,
                CanCheckout = true,
                IsPending = false,
                VehiclePicture = unlinked.VehiclePicture,
                AccurateTime = DateTime.Now,
                SnapTime =  DateTime.Now,
                CustomerCategory = category,
                LaneNO = input.WatcherCode
            };

            var reservationResult = await _garageTransactionUtilityAppService.ReserverSlotsOfGarageDependsOnCategoryAsync(
                entryDto.GarageId, 
                org.HasFreeSlots, 
                category);

            if (!reservationResult)
            {
                return Result<GarageTransactionDto>.Invalid(new ValidationError
                {
                    ErrorMessage = "Garage capacity is full or category capacity limit reached."
                });
            }

            var created = await _garageTransactionsAppService.CreateAsync(entryDto);

            await _unlinkedTransactionRepository.DeleteAsync(unlinked.Id);

            if (input.TriggerGateOpen)
            {
                try
                {
                    var tenantOrg = await _organizationAppService.GetOrganizationForCurrentTenatAsync();
                    var openGateService = await _openGateFactoryAppService.GetOpeningGateService(tenantOrg);
                    await openGateService.OpenGateAsync(new OpenGateDto
                    {
                        WatcherCode = input.WatcherCode,
                        OrganizationId = input.OrganizationId
                    });
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Failed to actuate gate barrier during unlinked recovery.");
                }
            }

            await CurrentUnitOfWork.SaveChangesAsync();

            return Result<GarageTransactionDto>.Success(created);
        }
    }
}

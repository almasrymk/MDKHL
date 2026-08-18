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
using SmartParking.Repositories;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace SmartParking.Mobile.Keepers;

[RemoteService(isEnabled: false)]
public class KeeperTransactionAppService : SmartParkingAppService, IKeeperTransactionAppService
{
    private readonly IMyUserRepository _myUserRepository;
    private readonly IRepository<GarageTransaction, int> _garageTransactionRepository;
    private readonly IRepository<UnLinkedTransaction, int> _unlinkedTransactionRepository;
    private readonly IRepository<Camera, int> _cameraRepository;
    private readonly IRepository<Watcher, int> _watcherRepository;
    private readonly IRepository<Organization, int> _organizationRepository;
    private readonly IGarageTransactionsAppService _garageTransactionsAppService;
    private readonly ICustomerPackageHandlerAppService _customerPackageHandlerAppService;
    private readonly IOpenGateFactoryService _openGateFactoryAppService;
    private readonly IOrganizationsAppService _organizationAppService;

    public KeeperTransactionAppService(
        IMyUserRepository myUserRepository,
        IRepository<GarageTransaction, int> garageTransactionRepository,
        IRepository<UnLinkedTransaction, int> unlinkedTransactionRepository,
        IRepository<Camera, int> cameraRepository,
        IRepository<Watcher, int> watcherRepository,
        IRepository<Organization, int> organizationRepository,
        IGarageTransactionsAppService garageTransactionsAppService,
        ICustomerPackageHandlerAppService customerPackageHandlerAppService,
        IOpenGateFactoryService openGateFactoryAppService,
        IOrganizationsAppService organizationAppService)
    {
        _myUserRepository = myUserRepository;
        _garageTransactionRepository = garageTransactionRepository;
        _unlinkedTransactionRepository = unlinkedTransactionRepository;
        _cameraRepository = cameraRepository;
        _watcherRepository = watcherRepository;
        _organizationRepository = organizationRepository;
        _garageTransactionsAppService = garageTransactionsAppService;
        _customerPackageHandlerAppService = customerPackageHandlerAppService;
        _openGateFactoryAppService = openGateFactoryAppService;
        _organizationAppService = organizationAppService;
    }

    /// <inheritdoc/>
    public async Task<Result<List<GarageTransactionDto>>> GetAllGarageTransactionsWithNoPlateNumberAsync(int organizationId, Guid userId)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var user = await _myUserRepository.FindUserByOrganizationAsync(organizationId, userId);
            if (user is null)
            {
                return Result.Invalid(new Results.ValidationError
                {
                    ErrorMessage = "User not found"
                });
            }

            var garageTransaction = await _garageTransactionRepository.GetListAsync(t => t.PlateNumber == null);
            var sortedTransaction = garageTransaction.OrderByDescending(t => t.AccurateTime).ToList();
            var mappedTransaction = ObjectMapper.Map<List<GarageTransaction>, List<GarageTransactionDto>>(sortedTransaction);

            if (mappedTransaction.Count < 0)
            {
                return Result<List<GarageTransactionDto>>.NotFound("Transaction not found");
            }

            return Result<List<GarageTransactionDto>>.Success(mappedTransaction);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<PagedResultDto<GarageTransactionDto>>> GetAllGarageTransactionsAsync(int organizationId, Guid userId, PagedAndSortedAndSearchResultRequestDto paged)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var user = await _myUserRepository.FindUserByOrganizationAsync(organizationId, userId);
            if (user is null)
            {
                return Result.Invalid(new Results.ValidationError
                {
                    ErrorMessage = "User not found"
                });
            }

            var query = await _garageTransactionRepository.GetQueryableAsync();
            query = query.OrderByDescending(t => t.AccurateTime);

            var totalCount = await AsyncExecuter.CountAsync(query);
            query = query.Skip(paged.SkipCount).Take(paged.MaxResultCount);

            var garageTransactions = await AsyncExecuter.ToListAsync(query);
            var mappedTransaction = ObjectMapper.Map<List<GarageTransaction>, List<GarageTransactionDto>>(garageTransactions);

            if (totalCount == 0)
            {
                return Result<PagedResultDto<GarageTransactionDto>>.NotFound("Transaction not found");
            }

            var result = new PagedResultDto<GarageTransactionDto>(totalCount, mappedTransaction);
            return Result<PagedResultDto<GarageTransactionDto>>.Success(result);
        }
    }

    /// <inheritdoc/>
    public async Task<Result> CorrectTransactionByPlateNumberAsync(int garageTransactionId, string plateNumber)
    {
        GarageTransaction? transaction;
        using (DataFilter.Disable<IMultiTenant>())
        {
            transaction = await _garageTransactionRepository.GetAsync(garageTransactionId);
        }

        if (transaction is null)
        {
            return Result.NotFound("transaction not found");
        }

        transaction.PlateNumber = plateNumber;
        try
        {
            await _garageTransactionRepository.UpdateAsync(transaction);
            return Result.SuccessWithMessage("PlateNumber corrected successfully");
        }
        catch (Exception ex)
        {
            return Result.Error("PlateNumber not corrected" + ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<PagedResultDto<GarageTransactionDto>>> GetAllExistingTransactionsAsync(int organizationId, Guid userId, PagedAndSortedAndSearchResultRequestDto paged)
    {
        var validationResult = await CheckUserIsFound(organizationId, userId);
        if (validationResult is not null)
        {
            return validationResult;
        }

        var existingTransactionsQuery = await _garageTransactionRepository.GetQueryableAsync();
        existingTransactionsQuery = existingTransactionsQuery.Where(t => t.IsIn).OrderByDescending(t => t.AccurateTime);

        var totalCount = await AsyncExecuter.CountAsync(existingTransactionsQuery);
        existingTransactionsQuery = existingTransactionsQuery.Skip(paged.SkipCount).Take(paged.MaxResultCount);

        var existingTransactions = await AsyncExecuter.ToListAsync(existingTransactionsQuery);
        var items = ObjectMapper.Map<List<GarageTransaction>, List<GarageTransactionDto>>(existingTransactions);
        var finalPaged = new PagedResultDto<GarageTransactionDto>(totalCount, items);

        return Result<PagedResultDto<GarageTransactionDto>>.Success(finalPaged);
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
                .Where(u => u.DeviceId == watcherCode && u.IsDeleted == false)
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

                // Plate is optional for dispenser transactions
                string rawPlate = (input.CorrectedPlateNumber ?? unlinked.PlateNumber ?? "").Trim().Replace(" ", "");
                if (!string.IsNullOrEmpty(rawPlate))
                {
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
                AccurateTime = unlinked.AccurateTime ?? unlinked.SnapTime ?? DateTime.Now,
                SnapTime = unlinked.SnapTime ?? unlinked.CreationTime,
                CustomerCategory = category,
                LaneNO = input.WatcherCode
            };

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

    #region Private Helpers

    private async Task<Result<PagedResultDto<GarageTransactionDto>>?> CheckUserIsFound(int organizationId, Guid userId)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var user = await _myUserRepository.FindUserByOrganizationAsync(organizationId, userId);
            if (user is null)
            {
                return Result<PagedResultDto<GarageTransactionDto>>.Invalid(new Results.ValidationError
                {
                    ErrorMessage = "User not found"
                });
            }
        }
        return null;
    }

    #endregion
}

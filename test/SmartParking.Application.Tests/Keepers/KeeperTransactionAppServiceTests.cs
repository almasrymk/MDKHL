using Microsoft.EntityFrameworkCore;
using Shouldly;
using SmartParking.Entities.Cameras.Watchers;
using SmartParking.Entities.GarageTransactions;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.Mobile.Keepers;
using SmartParking.Mobile.Keepers.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace SmartParking.Keepers;

public abstract class KeeperTransactionAppServiceTests<TStartupModule> : SmartParkingApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IUnlinkedTransactionRecoveryAppService _unlinkedTransactionRecoveryAppService;
    private readonly IKeeperTransactionQueryAppService _keeperTransactionQueryAppService;
    private readonly IKeeperTransactionCorrectionAppService _keeperTransactionCorrectionAppService;
    private readonly IRepository<Organization, int> _organizationRepository;
    private readonly IRepository<Dispenser, int> _dispenserRepository;
    private readonly IRepository<UnLinkedTransaction, int> _unlinkedTransactionRepository;
    private readonly IRepository<GarageTransaction, int> _garageTransactionRepository;
    private readonly IDataFilter _dataFilter;

    protected KeeperTransactionAppServiceTests()
    {
        _unlinkedTransactionRecoveryAppService = GetRequiredService<IUnlinkedTransactionRecoveryAppService>();
        _keeperTransactionQueryAppService = GetRequiredService<IKeeperTransactionQueryAppService>();
        _keeperTransactionCorrectionAppService = GetRequiredService<IKeeperTransactionCorrectionAppService>();
        _organizationRepository = GetRequiredService<IRepository<Organization, int>>();
        _dispenserRepository = GetRequiredService<IRepository<Dispenser, int>>();
        _unlinkedTransactionRepository = GetRequiredService<IRepository<UnLinkedTransaction, int>>();
        _garageTransactionRepository = GetRequiredService<IRepository<GarageTransaction, int>>();
        _dataFilter = GetRequiredService<IDataFilter>();
    }

    [Fact]
    public async Task GetLatestUnlinked_Should_Reject_Exit_Watcher()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            using (_dataFilter.Disable<IMultiTenant>())
            {
                var orgList = await _organizationRepository.GetListAsync();
                var targetOrg = orgList.FirstOrDefault();
                if (targetOrg != null)
                {
                    targetOrg.Settings = new OrganizationSettings { UnlinkedEntry = true };
                    await _organizationRepository.UpdateAsync(targetOrg, autoSave: true);

                    var exitWatcher = new Dispenser
                    {
                        Name = "Exit Watcher Test",
                        DeviceId = "EXIT_CAM_01",
                        CameraType = WatcherType.Exit,
                        OrganizationId = targetOrg.Id,
                        GarageId = 1,
                        Status = Status.Active
                    };
                    await _dispenserRepository.InsertAsync(exitWatcher, autoSave: true);

                    var result = await _unlinkedTransactionRecoveryAppService.GetLatestUnlinkedTransactionByWatcherCodeAsync(targetOrg.Id, "EXIT_CAM_01");

                    result.IsSuccess.ShouldBeFalse();
                    result.ValidationErrors.ShouldContain(v => v.ErrorMessage.Contains("Exit watcher"));
                }
            }
        });
    }

    [Fact]
    public async Task SendAsEntry_Should_Reject_When_No_Transaction_Selected()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            using (_dataFilter.Disable<IMultiTenant>())
            {
                var orgList = await _organizationRepository.GetListAsync();
                var targetOrg = orgList.FirstOrDefault();
                if (targetOrg != null)
                {
                    targetOrg.Settings = new OrganizationSettings { UnlinkedEntry = true };
                    await _organizationRepository.UpdateAsync(targetOrg, autoSave: true);

                    var input = new SendUnlinkedAsEntryDto
                    {
                        UnlinkedTransactionId = 0,
                        OrganizationId = targetOrg.Id,
                        WatcherCode = "CAM_01",
                        CorrectedPlateNumber = "ABC1234"
                    };

                    var result = await _unlinkedTransactionRecoveryAppService.SendUnlinkedTransactionAsEntryAsync(input);

                    result.IsSuccess.ShouldBeFalse();
                    result.ValidationErrors.ShouldContain(v => v.ErrorMessage.Contains("Transaction not found or has already been processed."));
                }
            }
        });
    }

    [Fact]
    public async Task SendAsEntry_Camera_Should_Reject_When_Plate_Empty()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            using (_dataFilter.Disable<IMultiTenant>())
            {
                var orgList = await _organizationRepository.GetListAsync();
                var targetOrg = orgList.FirstOrDefault();
                if (targetOrg != null)
                {
                    targetOrg.CameraTransaction = true;
                    targetOrg.DispenserTransaction = false;
                    targetOrg.Settings = new OrganizationSettings { UnlinkedEntry = true };
                    await _organizationRepository.UpdateAsync(targetOrg, autoSave: true);

                    var unlinked = new UnLinkedTransaction
                    {
                        DeviceId = "CAM_ENTRY_TEST",
                        PlateNumber = null,
                        SnapTime = DateTime.UtcNow,
                        VehiclePicture = "pic.jpg",
                        IsIn = true
                    };
                    var insertedUnlinked = await _unlinkedTransactionRepository.InsertAsync(unlinked, autoSave: true);

                    var watcher = new Dispenser
                    {
                        Name = "Entry Device Test",
                        DeviceId = "CAM_ENTRY_TEST",
                        CameraType = WatcherType.Entry,
                        OrganizationId = targetOrg.Id,
                        GarageId = 1,
                        Status = Status.Active
                    };
                    await _dispenserRepository.InsertAsync(watcher, autoSave: true);

                    var input = new SendUnlinkedAsEntryDto
                    {
                        UnlinkedTransactionId = insertedUnlinked.Id,
                        OrganizationId = targetOrg.Id,
                        WatcherCode = "CAM_ENTRY_TEST",
                        CorrectedPlateNumber = ""
                    };

                    var result = await _unlinkedTransactionRecoveryAppService.SendUnlinkedTransactionAsEntryAsync(input);

                    result.IsSuccess.ShouldBeFalse();
                    result.ValidationErrors.ShouldContain(v => v.ErrorMessage.Contains("Plate number is required."));
                }
            }
        });
    }

    [Fact]
    public async Task SendAsEntry_Dispenser_Should_Reject_When_Ticket_Empty()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            using (_dataFilter.Disable<IMultiTenant>())
            {
                var orgList = await _organizationRepository.GetListAsync();
                var targetOrg = orgList.FirstOrDefault();
                if (targetOrg != null)
                {
                    targetOrg.DispenserTransaction = true;
                    targetOrg.CameraTransaction = false;
                    targetOrg.Settings = new OrganizationSettings { UnlinkedEntry = true };
                    await _organizationRepository.UpdateAsync(targetOrg, autoSave: true);

                    var unlinked = new UnLinkedTransaction
                    {
                        DeviceId = "DISP_ENTRY_TEST",
                        PlateNumber = "1234XYZ",
                        SnapTime = DateTime.UtcNow,
                        VehiclePicture = "pic.jpg",
                        IsIn = true
                    };
                    var insertedUnlinked = await _unlinkedTransactionRepository.InsertAsync(unlinked, autoSave: true);

                    var dispenser = new Dispenser
                    {
                        Name = "Entry Dispenser Test",
                        DeviceId = "DISP_ENTRY_TEST",
                        CameraType = WatcherType.Entry,
                        OrganizationId = targetOrg.Id,
                        GarageId = 1,
                        Status = Status.Active
                    };
                    await _dispenserRepository.InsertAsync(dispenser, autoSave: true);

                    var input = new SendUnlinkedAsEntryDto
                    {
                        UnlinkedTransactionId = insertedUnlinked.Id,
                        OrganizationId = targetOrg.Id,
                        WatcherCode = "DISP_ENTRY_TEST",
                        CorrectedPlateNumber = "1234XYZ",
                        TicketNumber = ""
                    };

                    var result = await _unlinkedTransactionRecoveryAppService.SendUnlinkedTransactionAsEntryAsync(input);

                    result.IsSuccess.ShouldBeFalse();
                    result.ValidationErrors.ShouldContain(v => v.ErrorMessage.Contains("Ticket Number is required for dispenser transactions."));
                }
            }
        });
    }

    [Fact]
    public async Task SendAsEntry_Dispenser_Should_Succeed_When_Valid_Ticket_Provided()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            using (_dataFilter.Disable<IMultiTenant>())
            {
                var orgList = await _organizationRepository.GetListAsync();
                var targetOrg = orgList.FirstOrDefault();
                if (targetOrg != null)
                {
                    targetOrg.DispenserTransaction = true;
                    targetOrg.CameraTransaction = false;
                    targetOrg.Settings = new OrganizationSettings { UnlinkedEntry = true };
                    await _organizationRepository.UpdateAsync(targetOrg, autoSave: true);

                    var unlinked = new UnLinkedTransaction
                    {
                        DeviceId = "DISP_SUCCESS_01",
                        PlateNumber = null,
                        SnapTime = DateTime.UtcNow,
                        VehiclePicture = "pic.jpg",
                        IsIn = true
                    };
                    var insertedUnlinked = await _unlinkedTransactionRepository.InsertAsync(unlinked, autoSave: true);

                    var dispenser = new Dispenser
                    {
                        Name = "Success Dispenser Test",
                        DeviceId = "DISP_SUCCESS_01",
                        CameraType = WatcherType.Entry,
                        OrganizationId = targetOrg.Id,
                        GarageId = 1,
                        Status = Status.Active
                    };
                    await _dispenserRepository.InsertAsync(dispenser, autoSave: true);

                    var input = new SendUnlinkedAsEntryDto
                    {
                        UnlinkedTransactionId = insertedUnlinked.Id,
                        OrganizationId = targetOrg.Id,
                        WatcherCode = "DISP_SUCCESS_01",
                        CorrectedPlateNumber = null,
                        TicketNumber = "TCK-9999",
                        TriggerGateOpen = false
                    };

                    var result = await _unlinkedTransactionRecoveryAppService.SendUnlinkedTransactionAsEntryAsync(input);

                    result.IsSuccess.ShouldBeTrue();
                    result.Value.ShouldNotBeNull();
                    result.Value.TicketId.ShouldBe("TCK-9999");
                }
            }
        });
    }

    [Fact]
    public async Task Query_Should_Reject_When_User_Not_Found()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var fakeUserId = Guid.NewGuid();
            var result = await _keeperTransactionQueryAppService.GetAllGarageTransactionsWithNoPlateNumberAsync(1, fakeUserId);

            result.IsSuccess.ShouldBeFalse();
            result.ValidationErrors.ShouldContain(v => v.ErrorMessage.Contains("User not found"));
        });
    }

    [Fact]
    public async Task CorrectTransaction_Should_Return_NotFound_For_Invalid_Id()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var result = await _keeperTransactionCorrectionAppService.CorrectTransactionByPlateNumberAsync(999999, "XYZ-999");

            result.IsSuccess.ShouldBeFalse();
            result.Errors.ShouldContain(e => e.Contains("not found"));
        });
    }

    [Fact]
    public async Task CorrectTransaction_Should_Update_PlateNumber_Successfully()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            using (_dataFilter.Disable<IMultiTenant>())
            {
                var transaction = new GarageTransaction
                {
                    PlateNumber = "OLD123",
                    IsIn = true,
                    OrganizationID = 1,
                    GarageId = 1,
                    LaneNO = "LANE1"
                };
                var inserted = await _garageTransactionRepository.InsertAsync(transaction, autoSave: true);

                var result = await _keeperTransactionCorrectionAppService.CorrectTransactionByPlateNumberAsync(inserted.Id, "NEW456");

                result.IsSuccess.ShouldBeTrue();
                var updated = await _garageTransactionRepository.GetAsync(inserted.Id);
                updated.PlateNumber.ShouldBe("NEW456");
            }
        });
    }
}


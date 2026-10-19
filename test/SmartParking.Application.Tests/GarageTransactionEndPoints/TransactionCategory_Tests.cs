using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SmartParking.DispenserEndPoint;
using SmartParking.Entities.Cameras.Watchers;
using SmartParking.Entities.Customers;
using SmartParking.Entities.GarageTransactions;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Vehicles;
using SmartParking.Enums;
using SmartParking.GarageTransactionEndPoints;
using SmartParking.PackageCodeEndPoint;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace SmartParking.GarageTransactionEndPoints;

/// <summary>
/// Issue #31: the customer category of a transaction must be the same on entry and exit,
/// and the garage slot counters must return to their starting values after a full visit,
/// for every automated entry path (camera, dispenser ticket, dispenser package code).
/// </summary>
public abstract class TransactionCategory_Tests<TStartupModule> : SmartParkingApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private const string EntryCameraDeviceId = "233f1b26-9350-eb14-03e7-78934a049350";
    private const string ExitCameraDeviceId = "df9a5b33-adf7-f454-49dd-d9624605adf7";
    private const string EntryDispenserDeviceId = "TST-DISP-IN";
    private const string ExitDispenserDeviceId = "TST-DISP-OUT";

    private const string UnregisteredPlate = "9999UNR";
    private const string RegisteredPlate = "1111REG";
    private const string EmployeePlate = "2222EMP";
    private const string VisitorPlate = "3333VIS";
    private const string EmployeePackageCode = "EMP0000001";
    private const string VisitorPackageCode = "VIS0000001";

    private readonly IGarageTransactionEndPointsAppService _cameraEndPoints;
    private readonly IDispenserEndPointsAppService _dispenserEndPoints;
    private readonly IRepository<Organization, int> _organizationRepository;
    private readonly IRepository<Garage, int> _garageRepository;
    private readonly IRepository<Dispenser, int> _dispenserRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<Vehicle, int> _vehicleRepository;
    private readonly IRepository<CustomerPackage, int> _customerPackageRepository;
    private readonly IRepository<GarageTransaction, int> _garageTransactionRepository;
    private readonly IDataFilter _dataFilter;

    private string _organizationCode;
    private int _garageId;

    protected TransactionCategory_Tests()
    {
        _cameraEndPoints = GetRequiredService<IGarageTransactionEndPointsAppService>();
        _dispenserEndPoints = GetRequiredService<IDispenserEndPointsAppService>();
        _organizationRepository = GetRequiredService<IRepository<Organization, int>>();
        _garageRepository = GetRequiredService<IRepository<Garage, int>>();
        _dispenserRepository = GetRequiredService<IRepository<Dispenser, int>>();
        _customerRepository = GetRequiredService<IRepository<Customer, Guid>>();
        _vehicleRepository = GetRequiredService<IRepository<Vehicle, int>>();
        _customerPackageRepository = GetRequiredService<IRepository<CustomerPackage, int>>();
        _garageTransactionRepository = GetRequiredService<IRepository<GarageTransaction, int>>();
        _dataFilter = GetRequiredService<IDataFilter>();
    }

    // ── Camera ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Camera_UnregisteredCar_Should_Be_Customer_On_Entry_And_Exit()
    {
        await ArrangeAsync(cameraTransaction: true, dispenserTransaction: false);

        await CameraVisitAsync(UnregisteredPlate);

        await AssertVisitAsync(UnregisteredPlate, CustomerCategoryStatus.Customer);
    }

    [Fact]
    public async Task Camera_RegisteredCarWithoutPackage_Should_Be_Customer_On_Entry_And_Exit()
    {
        await ArrangeAsync(cameraTransaction: true, dispenserTransaction: false);

        await CameraVisitAsync(RegisteredPlate);

        await AssertVisitAsync(RegisteredPlate, CustomerCategoryStatus.Customer);
    }

    [Fact]
    public async Task Camera_EmployeePackage_Should_Be_Employee_On_Entry_And_Exit()
    {
        await ArrangeAsync(cameraTransaction: true, dispenserTransaction: false);

        await CameraVisitAsync(EmployeePlate);

        await AssertVisitAsync(EmployeePlate, CustomerCategoryStatus.Employee);
    }

    [Fact]
    public async Task Camera_VisitorPackage_Should_Be_Visitor_On_Entry_And_Exit()
    {
        await ArrangeAsync(cameraTransaction: true, dispenserTransaction: false);

        await CameraVisitAsync(VisitorPlate);

        await AssertVisitAsync(VisitorPlate, CustomerCategoryStatus.Visitor);
    }

    // ── Dispenser: numeric ticket ───────────────────────────────────────────

    [Fact]
    public async Task Dispenser_Ticket_UnregisteredCar_Should_Be_Customer_On_Entry_And_Exit()
    {
        await ArrangeAsync(cameraTransaction: false, dispenserTransaction: true);

        await TicketVisitAsync(UnregisteredPlate, "100001");

        await AssertVisitAsync(UnregisteredPlate, CustomerCategoryStatus.Customer);
    }

    [Fact]
    public async Task Dispenser_Ticket_RegisteredCarWithoutPackage_Should_Be_Customer_On_Entry_And_Exit()
    {
        await ArrangeAsync(cameraTransaction: false, dispenserTransaction: true);

        await TicketVisitAsync(RegisteredPlate, "100002");

        await AssertVisitAsync(RegisteredPlate, CustomerCategoryStatus.Customer);
    }

    // ── Dispenser: package code (QR) ────────────────────────────────────────

    [Fact]
    public async Task Dispenser_PackageCode_EmployeePackage_Should_Be_Employee_On_Entry_And_Exit()
    {
        await ArrangeAsync(cameraTransaction: false, dispenserTransaction: true);

        await PackageCodeVisitAsync(EmployeePlate, EmployeePackageCode);

        await AssertVisitAsync(EmployeePlate, CustomerCategoryStatus.Employee);
    }

    [Fact]
    public async Task Dispenser_PackageCode_VisitorPackage_Should_Be_Visitor_On_Entry_And_Exit()
    {
        await ArrangeAsync(cameraTransaction: false, dispenserTransaction: true);

        await PackageCodeVisitAsync(VisitorPlate, VisitorPackageCode);

        await AssertVisitAsync(VisitorPlate, CustomerCategoryStatus.Visitor);
    }

    // ── Mixed mode: camera and dispenser both enabled (production setup) ────

    [Fact]
    public async Task Mixed_Camera_RegisteredCarWithoutPackage_Should_Be_Customer_On_Entry_And_Exit()
    {
        await ArrangeAsync(cameraTransaction: true, dispenserTransaction: true);

        await CameraVisitAsync(RegisteredPlate);

        await AssertVisitAsync(RegisteredPlate, CustomerCategoryStatus.Customer);
    }

    [Fact]
    public async Task Mixed_Camera_EmployeePackage_Should_Be_Employee_On_Entry_And_Exit()
    {
        await ArrangeAsync(cameraTransaction: true, dispenserTransaction: true);

        await CameraVisitAsync(EmployeePlate);

        await AssertVisitAsync(EmployeePlate, CustomerCategoryStatus.Employee);
    }

    [Fact]
    public async Task Mixed_Dispenser_Ticket_RegisteredCarWithoutPackage_Should_Be_Customer_On_Entry_And_Exit()
    {
        await ArrangeAsync(cameraTransaction: true, dispenserTransaction: true);

        await TicketVisitAsync(RegisteredPlate, "100003");

        await AssertVisitAsync(RegisteredPlate, CustomerCategoryStatus.Customer);
    }

    [Fact]
    public async Task Mixed_Dispenser_PackageCode_VisitorPackage_Should_Be_Visitor_On_Entry_And_Exit()
    {
        await ArrangeAsync(cameraTransaction: true, dispenserTransaction: true);

        await PackageCodeVisitAsync(VisitorPlate, VisitorPackageCode);

        await AssertVisitAsync(VisitorPlate, CustomerCategoryStatus.Visitor);
    }

    // ── Package-code endpoint (separate API) ────────────────────────────────

    [Fact]
    public async Task PackageCodeEndPoint_EmployeePackage_Should_Be_Employee_On_Entry_And_Exit()
    {
        await ArrangeAsync(cameraTransaction: false, dispenserTransaction: true);
        var packageCodeEndPoint = GetRequiredService<IPackageCodeEndpointAppService>();

        await packageCodeEndPoint.PackageCodeEntry(new PackageCodeEntryDto
        {
            CarNum = EmployeePlate,
            LaneNO = EntryDispenserDeviceId,
            TicketID = EmployeePackageCode,
            PackageCode = EmployeePackageCode,
            ActTime = DateTime.Now.AddHours(-2).ToString("yyyyMMddHHmmss")
        });
        await packageCodeEndPoint.PackageCodeExit(new PackageCodeExitDto
        {
            CarNum = EmployeePlate,
            LaneNO = ExitDispenserDeviceId,
            TicketID = EmployeePackageCode,
            PackageCode = EmployeePackageCode,
            ActTime = DateTime.Now.AddHours(-1).ToString("yyyyMMddHHmmss")
        });

        await AssertVisitAsync(EmployeePlate, CustomerCategoryStatus.Employee);
    }

    // ── Visits ──────────────────────────────────────────────────────────────

    private async Task CameraVisitAsync(string plate)
    {
        var entryTime = DateTime.Now.AddHours(-2);
        await _cameraEndPoints.CreateAsync(new CreateUpdateGarageTransactionEndPointDto
        {
            DeviceID = EntryCameraDeviceId,
            PlateNumber = plate,
            AccurateTime = entryTime,
            SnapTime = entryTime
        });

        var exitTime = DateTime.Now.AddHours(-1);
        await _cameraEndPoints.CreateAsync(new CreateUpdateGarageTransactionEndPointDto
        {
            DeviceID = ExitCameraDeviceId,
            PlateNumber = plate,
            AccurateTime = exitTime,
            SnapTime = exitTime
        });
    }

    private async Task TicketVisitAsync(string plate, string ticket)
    {
        await _dispenserEndPoints.Entry(_organizationCode, new EntryDto
        {
            CarNum = plate,
            LaneNO = EntryDispenserDeviceId,
            TicketID = ticket,
            ActTime = DateTime.Now.AddHours(-2).ToString("yyyyMMddHHmmss")
        });

        await _dispenserEndPoints.Exit(_organizationCode, new ExitDto
        {
            CarNum = plate,
            LaneNO = ExitDispenserDeviceId,
            TicketId = ticket,
            ActTime = DateTime.Now.AddHours(-1).ToString("yyyyMMddHHmmss")
        });
    }

    private async Task PackageCodeVisitAsync(string plate, string packageCode)
    {
        await _dispenserEndPoints.PreEntry(_organizationCode, new PackageCodeEntryDto
        {
            CarNum = plate,
            LaneNO = EntryDispenserDeviceId,
            TicketID = packageCode,
            PackageCode = packageCode,
            ActTime = DateTime.Now.AddHours(-2).ToString("yyyyMMddHHmmss")
        });

        await _dispenserEndPoints.PreExit(_organizationCode, new PackageCodeExitDto
        {
            CarNum = plate,
            LaneNO = ExitDispenserDeviceId,
            TicketID = packageCode,
            PackageCode = packageCode,
            ActTime = DateTime.Now.AddHours(-1).ToString("yyyyMMddHHmmss")
        });
    }

    // ── Assertions ──────────────────────────────────────────────────────────

    private async Task AssertVisitAsync(string plate, CustomerCategoryStatus expectedCategory)
    {
        await WithUnitOfWorkAsync(async () =>
        {
            using (_dataFilter.Disable<IMultiTenant>())
            {
                var transactions = await _garageTransactionRepository.GetListAsync(x => x.PlateNumber == plate);

                var entry = transactions.SingleOrDefault(x => x.CameraType == WatcherType.Entry);
                var exit = transactions.SingleOrDefault(x => x.CameraType == WatcherType.Exit);

                entry.ShouldNotBeNull("entry transaction was not created");
                exit.ShouldNotBeNull("exit transaction was not created");
                entry.IsMapped.ShouldBeTrue("entry was not closed by the exit");

                var garage = await _garageRepository.GetAsync(_garageId);
                var actual = $"entry={entry.CustomerCategory}, exit={exit.CustomerCategory}, " +
                             $"slots(customers/employees/visitors/total)={garage.CurrnetCapacityForCustomers}/{garage.CurrnetCapacityEmployees}/{garage.CurrnetCapacityForFree}/{garage.CurrnetCapicity}";

                actual.ShouldBe($"entry={expectedCategory}, exit={expectedCategory}, slots(customers/employees/visitors/total)=0/0/0/0");
            }
        });
    }

    // ── Arrange ─────────────────────────────────────────────────────────────

    private async Task ArrangeAsync(bool cameraTransaction, bool dispenserTransaction)
    {
        await WithUnitOfWorkAsync(async () =>
        {
            using (_dataFilter.Disable<IMultiTenant>())
            {
                var organization = (await _organizationRepository.GetListAsync()).First();
                organization.TenantId ??= Guid.NewGuid();
                organization.Settings ??= new OrganizationSettings();
                organization.CameraTransaction = cameraTransaction;
                organization.DispenserTransaction = dispenserTransaction;
                organization.HasFreeSlots = false;
                organization.HasPackageOnly = false;
                organization.FreeMinutes = 0;
                organization.Graceperiod = 0;
                await _organizationRepository.UpdateAsync(organization, autoSave: true);
                _organizationCode = organization.Code;

                var garage = await _garageRepository.FirstAsync(x => x.NameEn == "Main Garage");
                garage.MaxCapacity = 150;
                garage.MaxSlotsForCustomers = 50;
                garage.MaxSlotsForEmployess = 50;
                garage.MaxSlotsForFree = 50;
                garage.CurrnetCapicity = 0;
                garage.CurrnetCapacityForCustomers = 0;
                garage.CurrnetCapacityEmployees = 0;
                garage.CurrnetCapacityForFree = 0;
                await _garageRepository.UpdateAsync(garage, autoSave: true);
                _garageId = garage.Id;

                await _dispenserRepository.InsertManyAsync(new[]
                {
                    new Dispenser { Name = "Entry dispenser", DeviceId = EntryDispenserDeviceId, CameraType = WatcherType.Entry, GarageId = garage.Id, OrganizationId = organization.Id, Status = Status.Active },
                    new Dispenser { Name = "Exit dispenser", DeviceId = ExitDispenserDeviceId, CameraType = WatcherType.Exit, GarageId = garage.Id, OrganizationId = organization.Id, Status = Status.Active }
                }, autoSave: true);

                var customer = (await _customerRepository.GetListAsync()).First();
                var template = (await _vehicleRepository.GetListAsync()).First();
                foreach (var plate in new[] { RegisteredPlate, EmployeePlate, VisitorPlate })
                {
                    await _vehicleRepository.InsertAsync(new Vehicle
                    {
                        CustomerId = customer.Id,
                        VehicleModelId = template.VehicleModelId,
                        VehicleBrandId = template.VehicleBrandId,
                        PlateColorId = template.PlateColorId,
                        LicenceTypeId = template.LicenceTypeId,
                        EnPlateNumber = plate,
                        ArPlateNumber = plate,
                        ModelYear = "2024",
                        Status = Status.Active
                    }, autoSave: true);
                }

                await _customerPackageRepository.InsertAsync(BuildActivePackage(customer.Id, organization.Id, EmployeePlate, EmployeePackageCode, CustomerCategoryStatus.Employee), autoSave: true);
                await _customerPackageRepository.InsertAsync(BuildActivePackage(customer.Id, organization.Id, VisitorPlate, VisitorPackageCode, CustomerCategoryStatus.Visitor), autoSave: true);
            }
        });
    }

    private static CustomerPackage BuildActivePackage(Guid customerId, int organizationId, string plate, string packageCode, CustomerCategoryStatus category) =>
        new()
        {
            CustomerId = customerId,
            OrganizationId = organizationId,
            EnPlateNumber = plate,
            ArPlateNumber = plate,
            PackageCode = packageCode,
            CustomerCategory = category,
            CustomerPackageStatus = CustomerPackageStatus.Active,
            Status = Status.Active,
            ActivationDateOfPackage = DateTime.Now.AddDays(-10),
            DectivationDateOfPackage = DateTime.Now.AddDays(30),
            ExpiryDate = DateTime.Now.AddDays(30),
            publishedOfPackage = true,
            PriceOfPackage = 0,
            PackageArabicName = plate,
            PackageEnglishName = plate
        };
}

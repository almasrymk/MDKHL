using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using MudBlazor;
using SmartParking.Localization;
using SmartParking.MultiTenancy;
using SmartParking.Organizations;
using SmartParking.Permissions.CameraPermissions;
using SmartParking.Permissions.ContactPermissions;
using SmartParking.Permissions.CustomerPermissions;
using SmartParking.Permissions.DailyShiftsPermissions;
using SmartParking.Permissions.DispenserPermissions;
using SmartParking.Permissions.GaragePermissions;
using SmartParking.Permissions.GarageTransactionPermissions;
using SmartParking.Permissions.InvoicesPermissions;
using SmartParking.Permissions.MyAccount;
using SmartParking.Permissions.MyShiftsPermissions;
using SmartParking.Permissions.OrganizationPermissions;
using SmartParking.Permissions.PackagePermissions;
using SmartParking.Permissions.PaymentsPermissions;
using SmartParking.Permissions.Reports;
using SmartParking.Permissions.ShiftsPermissions;
using SmartParking.Permissions.SubsctriptionPermissions;
using SmartParking.Permissions.UnLinkedTransactionPermissions;
using SmartParking.Permissions.UnlinkedEntryPermissions;
using SmartParking.Permissions.UnitsPermissions;
using SmartParking.Permissions.UnitVehiclePackagesPermissions;
using SmartParking.Permissions.UserProfile;
using SmartParking.Permissions.UserShiftsPermissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.UI.Navigation;

namespace SmartParking.Blazor.Menus;

public class SmartParkingMenuContributor : IMenuContributor
{
    public SmartParkingMenuContributor()
    {
    }

    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            await ConfigureMainMenuAsync(context);
        }
    }

    private async Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var tenant = context.ServiceProvider.GetRequiredService<ICurrentTenant>();
        var orgService = context.ServiceProvider.GetRequiredService<IOrganizationsAppService>();
        var org = await orgService.GetOrganizationForCurrentTenatAsync();
        var l = context.GetLocalizer<SmartParkingResource>();

        AddDashboardAndAccount(context, l, tenant);
        AddOperationalMenus(context, l, tenant, org);
        AddShiftsMenu(context, l, tenant, org);
        AddDeviceMenus(context, l, tenant, org);
        AddSubscriptionMenus(context, l, tenant);
        AddPackageMenus(context, l, tenant, org);
        AddTransactionMenus(context, l, tenant, org);
        await AddReportMenusAsync(context, l, tenant, org);
        AddLookupMenus(context, l, tenant);
        AddSettingsMenus(context, l, tenant);

        context.Menu.TryRemoveMenuItem(DefaultMenuNames.Application.Main.Administration);
    }

    private  void AddDashboardAndAccount(
        MenuConfigurationContext context,
        IStringLocalizer l,
        ICurrentTenant tenant)
    {
        context.Menu.AddItem(
            new ApplicationMenuItem(
                SmartParkingMenus.Dashboard,
                l["Dashboard"],
                "/",
                icon: Icons.Material.Filled.InsertChartOutlined,
                order: 1
            )
        );

        if (tenant.Id != null)
        {
            context.Menu.AddItem(
                new ApplicationMenuItem(
                    "SmartParking.MyAccount",
                    l["ViewMyAccount"],
                    "/view-my-account",
                    icon: Icons.Material.Filled.LocationCity,
                    order: 2
                ).RequirePermissions(MyAccountPermissions.Default)
            );
        }
    }

    private  void AddOperationalMenus(
        MenuConfigurationContext context,
        IStringLocalizer l,
        ICurrentTenant tenant,
        OrganizationDto? org)
    {
        context.Menu.AddItem(
            new ApplicationMenuItem(
                "SmartParking.Customers",
                l["Customers"],
                "/customers",
                icon: Icons.Material.Filled.Man2,
                order: 3
            ).RequirePermissions(CustomerPermissions.Default)
        );

        if (tenant.Id == null)
        {
            context.Menu.AddItem(
                new ApplicationMenuItem(
                    "SmartParking.Organizations",
                    l["Organizations"],
                    "/organizations",
                    icon: Icons.Material.Filled.LocationCity,
                    order: 4
                ).RequirePermissions(OrganizationPermissions.Default)
            );
        }

        context.Menu.AddItem(
            new ApplicationMenuItem(
                "SmartParking.Garages",
                l["Garages"],
                "/garages",
                icon: Icons.Material.Filled.Garage,
                order: 5
            ).RequirePermissions(GaragePermissions.Default)
        );

        if (tenant.Id != null)
        {
            context.Menu.AddItem(
                new ApplicationMenuItem(
                    "SmartParking.LastInvoice",
                    l["LastInvoice"],
                    "/lastinvoice",
                    icon: Icons.Material.Filled.Check,
                    order: 6
                )
            );

            bool isUnlinkedEntry = org?.OrganizationSettings?.UnlinkedEntry ?? false;
            if (isUnlinkedEntry)
            {
                context.Menu.AddItem(
                    new ApplicationMenuItem(
                        "SmartParking.UnlinkedEntry",
                        l["UnlinkedEntry"],
                        "/unlinked-at-camera",
                        icon: Icons.Material.Filled.CameraEnhance,
                        order: 7
                    ).RequirePermissions(UnlinkedEntryPermissions.Default)
                );
            }
        }
    }

    private  void AddShiftsMenu(
        MenuConfigurationContext context,
        IStringLocalizer l,
        ICurrentTenant tenant,
        OrganizationDto? org)
    {
        bool isShift = org?.OrganizationSettings?.UseShifts ?? true;
        if (!isShift)
        {
            return;
        }

        var shiftsGroup = new ApplicationMenuItem(
            SmartParkingMenus.Shifts,
            l["Shifts"],
            icon: Icons.Material.Filled.FilterTiltShift,
            order: 8
        );

        if (tenant.Id == null)
        {
            shiftsGroup.AddItem(
                new ApplicationMenuItem(
                    "SmartParking.Shifts.Add",
                    l["AddShifts"],
                    "/shifts",
                    icon: Icons.Material.Filled.ViewTimeline
                )
            );

            shiftsGroup.AddItem(
                new ApplicationMenuItem(
                    "SmartParking.Shifts.Users",
                    l["UsersCanShift"],
                    "/user/shifts",
                    icon: Icons.Material.Filled.Person2
                )
            );
        }
        else
        {
            shiftsGroup.AddItem(
                new ApplicationMenuItem(
                    "SmartParking.Shifts.Add",
                    l["AddShifts"],
                    "/shifts",
                    icon: Icons.Material.Filled.ViewTimeline
                ).RequirePermissions(ShiftPermission.Default)
            );

            shiftsGroup.AddItem(
                new ApplicationMenuItem(
                    "SmartParking.Shifts.Users",
                    l["UsersCanShift"],
                    "/user/shifts",
                    icon: Icons.Material.Filled.Person2
                ).RequirePermissions(UserShiftPermission.Default)
            );

            shiftsGroup.AddItem(
                new ApplicationMenuItem(
                    "SmartParking.Shifts.Zones",
                    l["ShiftZones"],
                    "/shift-zone",
                    icon: Icons.Material.Filled.Schedule
                ).RequirePermissions(MyShiftPermissions.Default)
            );
        }

        context.Menu.AddItem(shiftsGroup);
    }

    private  void AddDeviceMenus(
        MenuConfigurationContext context,
        IStringLocalizer l,
        ICurrentTenant tenant,
        OrganizationDto? org)
    {
        bool isCamera = org?.CameraTransaction ?? true;
        if (isCamera)
        {
            context.Menu.AddItem(
                new ApplicationMenuItem(
                    "SmartParking.Cameras",
                    l["Cameras"],
                    "/cameras",
                    icon: Icons.Material.Filled.CameraAlt,
                    order: 9
                ).RequirePermissions(CameraPermissions.Default)
            );
        }

        bool isDispenser = org?.DispenserTransaction ?? true;
        if (isDispenser)
        {
            if (tenant.Id == null)
            {
                context.Menu.AddItem(
                    new ApplicationMenuItem(
                        "SmartParking.Dispensers",
                        l["Dispensers"],
                        "/dispensers",
                        icon: Icons.Material.Filled.EmojiTransportation,
                        order: 10
                    )
                );
            }
            else
            {
                context.Menu.AddItem(
                    new ApplicationMenuItem(
                        "SmartParking.Dispensers",
                        l["Dispensers"],
                        "/dispensers",
                        icon: Icons.Material.Filled.EmojiTransportation,
                        order: 10
                    ).RequirePermissions(DispenserPermissions.Default)
                );
            }
        }
    }

    private  void AddSubscriptionMenus(
        MenuConfigurationContext context,
        IStringLocalizer l,
        ICurrentTenant tenant)
    {
        if (tenant.Id == null)
        {
            context.Menu.AddItem(
                new ApplicationMenuItem(
                    "SmartParking.Subscriptions",
                    l["Subscriptions"],
                    "/subscriptions",
                    icon: Icons.Material.Filled.Group,
                    order: 11
                ).RequirePermissions(SubscriptionPermissions.Default)
            );

            context.Menu.AddItem(
                new ApplicationMenuItem(
                    "SmartParking.SubscriptionTypes",
                    l["SubscriptionTypes"],
                    "/subscriptiontypes",
                    icon: Icons.Material.Filled.Group,
                    order: 12
                ).RequirePermissions(SubscriptionPermissions.Default)
            );
        }
        else
        {
            context.Menu.AddItem(
                new ApplicationMenuItem(
                    "SmartParking.Subscription",
                    l["Subscriptions"],
                    "/subscription",
                    icon: Icons.Material.Filled.Group,
                    order: 19
                ).RequirePermissions(SubscriptionPermissions.Default)
            );
        }
    }

    private  void AddPackageMenus(
        MenuConfigurationContext context,
        IStringLocalizer l,
        ICurrentTenant tenant,
        OrganizationDto? org)
    {
        if (tenant.Id == null)
        {
            context.Menu.AddItem(
                new ApplicationMenuItem(
                    "SmartParking.Packages",
                    l["Packages"],
                    "/package",
                    icon: Icons.Material.Filled.Subscriptions,
                    order: 13
                ).RequirePermissions(PackagePermissions.Default)
            );

            context.Menu.AddItem(
                new ApplicationMenuItem(
                    "SmartParking.UnitPackages",
                    l["UnitPackages"],
                    "/unit-package",
                    icon: Icons.Material.Filled.Subscriptions,
                    order: 14
                ).RequirePermissions(PackagePermissions.Default)
            );
        }
        else
        {
            bool isUserAccess = org?.OrganizationSettings?.UserAccess ?? true;
            string packageUrl = !isUserAccess ? "/package" : "/unit-package";

            context.Menu.AddItem(
                new ApplicationMenuItem(
                    "SmartParking.Packages",
                    l["Packages"],
                    packageUrl,
                    icon: Icons.Material.Filled.Subscriptions,
                    order: 13
                ).RequirePermissions(PackagePermissions.Default)
            );
        }
    }

    private  void AddTransactionMenus(
        MenuConfigurationContext context,
        IStringLocalizer l,
        ICurrentTenant tenant,
        OrganizationDto? org)
    {
        context.Menu.AddItem(
            new ApplicationMenuItem(
                "SmartParking.Invoices",
                l["Invoices"],
                "/invoices",
                icon: Icons.Material.Filled.Assignment,
                order: 15
            ).RequirePermissions(InvoicePermission.Default)
        );

        context.Menu.AddItem(
            new ApplicationMenuItem(
                "SmartParking.Payments",
                l["Payments"],
                "/payments",
                icon: Icons.Material.Filled.Money,
                order: 16
            ).RequirePermissions(PaymentsPermission.Default)
        );

        context.Menu.AddItem(
            new ApplicationMenuItem(
                "SmartParking.GarageTransactions",
                l["GarageTransactions"],
                "/garageTransactions",
                icon: Icons.Material.Filled.CompareArrows,
                order: 17
            ).RequirePermissions(GarageTransactionPermissions.Default)
        );

        if (tenant.Id == null)
        {
            context.Menu.AddItem(
                new ApplicationMenuItem(
                    "SmartParking.UnLinkedTransaction",
                    l["UnLinkedTransaction"],
                    "/unLinkedTransaction",
                    icon: Icons.Material.Filled.Garage,
                    order: 18
                ).RequirePermissions(UnLinkedTransactionPermission.Default)
            );
        }

        bool isUserAccess = org?.OrganizationSettings?.UserAccess ?? true;
        if (org != null && isUserAccess)
        {
            context.Menu.AddItem(
                new ApplicationMenuItem(
                    "SmartParking.Units",
                    l["Units"],
                    "/units",
                    icon: Icons.Material.Filled.House,
                    order: 20
                ).RequirePermissions(UnitPermissions.Default)
            );
        }
    }

    private  async Task AddReportMenusAsync(
        MenuConfigurationContext context,
        IStringLocalizer l,
        ICurrentTenant tenant,
        OrganizationDto? org)
    {
        bool isHost = tenant.Id == null;
        bool hasAnyReportPerm = isHost
            || await context.IsGrantedAsync(ReportsPermissions.SecurityManager)
            || await context.IsGrantedAsync(ReportsPermissions.IT)
            || await context.IsGrantedAsync(ReportsPermissions.FinancialManagement);

        if (!hasAnyReportPerm)
        {
            return;
        }

        var reportsGroup = new ApplicationMenuItem(
            SmartParkingMenus.Reports,
            l["Reports"],
            icon: Icons.Material.Filled.Assessment,
            order: 21
        );

        bool isShift = org?.OrganizationSettings?.UseShifts ?? true;

        // Security / IT reports
        AddSecurityReportItem(reportsGroup, "SmartParking.Reports.Beneficiaries", l["GarageTransactions"], "/beneficiariesreport", Icons.Material.Filled.People, isHost);
        AddSecurityReportItem(reportsGroup, "SmartParking.Reports.Customer", l["CustomerAndTransactionReport"], "/customerreport", Icons.Material.Filled.Man, isHost);
        AddSecurityReportItem(reportsGroup, "SmartParking.Reports.Employee", l["EmployeeAndTransactionReport"], "/employeereport", Icons.Material.Filled.Work, isHost);
        AddSecurityReportItem(reportsGroup, "SmartParking.Reports.Visitor", l["VisitorAndTransactionReport"], "/visitorreport", Icons.Material.Filled.Accessibility, isHost);

        if (isShift)
        {
            AddSecurityReportItem(reportsGroup, "SmartParking.Reports.KeeperTransactions", l["KeeperTransactions"], "/keepertransactionsreport", Icons.Material.Filled.CompareArrows, isHost);
        }

        AddSecurityReportItem(reportsGroup, "SmartParking.Reports.Vehicles", l["BeneficiariesVehiclesReport"], "/beneficiariesvehiclesreport", Icons.Material.Filled.CarCrash, isHost);
        AddSecurityReportItem(reportsGroup, "SmartParking.Reports.Details", l["BeneficiariesDetailsReports"], "/beneficiariesdetailsreport", Icons.Material.Filled.Group, isHost);
        AddSecurityReportItem(reportsGroup, "SmartParking.Reports.CancelledGarageTx", l["CancelledGarageTransactionReport"], "/cancelledgaragetransactionreport", Icons.Material.Filled.AssignmentReturned, isHost);
        AddSecurityReportItem(reportsGroup, "SmartParking.Reports.QRTransaction", l["QRTransactionReport"], "/qrtransactionreport", Icons.Material.Filled.QrCode2, isHost);

        // SecurityManager only reports
        AddSecurityManagerOnlyReportItem(reportsGroup, "SmartParking.Reports.PackageSubscribers", l["PackageSubscribers"], "/customerpackage/reports", Icons.Material.Filled.Subscriptions, isHost);
        AddSecurityManagerOnlyReportItem(reportsGroup, "SmartParking.Reports.CustomerPackage", l["CustomerPackageReport"], "/customerspackage/reports", Icons.Material.Filled.Subscriptions, isHost);

        // Financial reports
        if (isShift)
        {
            AddFinancialReportItem(reportsGroup, "SmartParking.Reports.KeeperPayments", l["KeeperPayments"], "/keeperpaymentsreport", Icons.Material.Filled.MoreTime, isHost);
        }

        AddFinancialReportItem(reportsGroup, "SmartParking.Reports.PackagePaymentsData", l["PackagePaymentsData"], "/packagepayment/reports", Icons.Material.Filled.MoneyOff, isHost);
        AddFinancialReportItem(reportsGroup, "SmartParking.Reports.Payments", l["Payments"], "/paymentsreports", Icons.Material.Filled.Money, isHost);
        AddFinancialReportItem(reportsGroup, "SmartParking.Reports.PackagesPayments", l["PackagesPayments"], "/packagepaymentreport", Icons.Material.Filled.Money, isHost);
        AddFinancialReportItem(reportsGroup, "SmartParking.Reports.PaymentMethod", l["PaymentMehtodReport"], "/paymentsbankreports", Icons.Material.Filled.Payment, isHost);
        AddFinancialReportItem(reportsGroup, "SmartParking.Reports.CustomerAndPayment", l["CustomerAndPaymentReport"], "/paymentscustomerreports", Icons.Material.Filled.AttachMoney, isHost);
        AddFinancialReportItem(reportsGroup, "SmartParking.Reports.Discounts", l["DiscountsReport"], "/discount/report", Icons.Material.Filled.Discount, isHost);

        bool isUserAccess = org?.OrganizationSettings?.UserAccess ?? true;
        if (org != null && isUserAccess)
        {
            reportsGroup.AddItem(
                new ApplicationMenuItem(
                    "SmartParking.Reports.UnitVehiclePackages",
                    l["UnitVehiclePackages"],
                    "/unit-vehicle-packages",
                    icon: Icons.Material.Filled.DataThresholding
                ).RequirePermissions(UnitVehiclePackagePermissions.Default)
            );

            reportsGroup.AddItem(
                new ApplicationMenuItem(
                    "SmartParking.Reports.CustomerUnitPackage",
                    l["CustomerUnitPackage"],
                    "/customer-unit-packages",
                    icon: Icons.Material.Filled.DataThresholding
                ).RequirePermissions(UnitVehiclePackagePermissions.Default)
            );

            reportsGroup.AddItem(
                new ApplicationMenuItem(
                    "SmartParking.Reports.PronightBooking",
                    l["PronightBooking"],
                    "/pronight-packages",
                    icon: Icons.Material.Filled.DataThresholding
                ).RequirePermissions(UnitVehiclePackagePermissions.Default)
            );
        }

        context.Menu.AddItem(reportsGroup);
    }

    private  void AddSecurityReportItem(
        ApplicationMenuItem parent,
        string name,
        string displayName,
        string url,
        string icon,
        bool isHost)
    {
        var item = new ApplicationMenuItem(name, displayName, url, icon: icon);
        if (!isHost)
        {
            item.RequirePermissions(false, ReportsPermissions.SecurityManager, ReportsPermissions.IT);
        }
        parent.AddItem(item);
    }

    private  void AddSecurityManagerOnlyReportItem(
        ApplicationMenuItem parent,
        string name,
        string displayName,
        string url,
        string icon,
        bool isHost)
    {
        var item = new ApplicationMenuItem(name, displayName, url, icon: icon);
        if (!isHost)
        {
            item.RequirePermissions(ReportsPermissions.SecurityManager);
        }
        parent.AddItem(item);
    }

    private  void AddFinancialReportItem(
        ApplicationMenuItem parent,
        string name,
        string displayName,
        string url,
        string icon,
        bool isHost)
    {
        var item = new ApplicationMenuItem(name, displayName, url, icon: icon);
        if (!isHost)
        {
            item.RequirePermissions(ReportsPermissions.FinancialManagement);
        }
        parent.AddItem(item);
    }

    private  void AddLookupMenus(
        MenuConfigurationContext context,
        IStringLocalizer l,
        ICurrentTenant tenant)
    {
        if (tenant.Id != null)
        {
            return;
        }

        var lookupsGroup = new ApplicationMenuItem(
            SmartParkingMenus.Lookups,
            l["Lookups"],
            icon: Icons.Material.Filled.Assignment,
            order: 22
        );

        lookupsGroup.AddItem(new ApplicationMenuItem("SmartParking.Lookups.Countries", l["Countries"], "/countries", icon: Icons.Material.Filled.Public));
        lookupsGroup.AddItem(new ApplicationMenuItem("SmartParking.Lookups.Regions", l["Regions"], "/regions", icon: Icons.Material.Filled.Public));
        lookupsGroup.AddItem(new ApplicationMenuItem("SmartParking.Lookups.States", l["States"], "/states", icon: Icons.Material.Filled.Place));
        lookupsGroup.AddItem(new ApplicationMenuItem("SmartParking.Lookups.Cities", l["Cities"], "/cities", icon: Icons.Material.Filled.LocationCity));
        lookupsGroup.AddItem(new ApplicationMenuItem("SmartParking.Lookups.PackageTypes", l["PackageTypes"], "/packagetypes", icon: Icons.Material.Filled.AllInbox));
        lookupsGroup.AddItem(new ApplicationMenuItem("SmartParking.Lookups.PlateColors", l["PlateColors"], "/plateColors", icon: Icons.Material.Filled.BorderColor));
        lookupsGroup.AddItem(new ApplicationMenuItem("SmartParking.Lookups.LicenceTypes", l["LicenceTypes"], "/licenceTypes", icon: Icons.Material.Filled.PermIdentity));
        lookupsGroup.AddItem(new ApplicationMenuItem("SmartParking.Lookups.VehicleBrands", l["VehicleBrands"], "/vehicleBrands", icon: Icons.Material.Filled.DirectionsCar));
        lookupsGroup.AddItem(new ApplicationMenuItem("SmartParking.Lookups.VehicleModels", l["VehicleModels"], "/vehicleModels", icon: Icons.Material.Filled.ElectricCar));
        lookupsGroup.AddItem(new ApplicationMenuItem("SmartParking.Lookups.CameraBrands", l["CameraBrands"], "/Camerabrands", icon: Icons.Material.Filled.ElectricCar));
        lookupsGroup.AddItem(new ApplicationMenuItem("SmartParking.Lookups.CameraModels", l["CameraModels"], "/CameraModels", icon: Icons.Material.Filled.ElectricCar));
        lookupsGroup.AddItem(new ApplicationMenuItem("SmartParking.Lookups.CameraMainTypes", l["CameraMainTypes"], "/Cameramaintypes", icon: Icons.Material.Filled.ElectricCar));

        context.Menu.AddItem(lookupsGroup);
    }

    private  void AddSettingsMenus(
        MenuConfigurationContext context,
        IStringLocalizer l,
        ICurrentTenant tenant)
    {
        var settingsGroup = new ApplicationMenuItem(
            SmartParkingMenus.Settings,
            l["Settings"],
            icon: Icons.Material.Filled.Settings,
            order: 23
        );

        settingsGroup.AddItem(
            new ApplicationMenuItem(
                "SmartParking.Settings.Profile",
                l["Profile"],
                "/edit-profile",
                icon: Icons.Material.Filled.AccountCircle
            ).RequirePermissions(UserProfilePermissions.View)
        );

        settingsGroup.AddItem(
            new ApplicationMenuItem(
                "SmartParking.Settings.Users",
                l["Users"],
                "/users",
                icon: Icons.Material.Filled.Man2
            ).RequirePermissions(IdentityPermissions.Users.Default)
        );

        if (tenant.Id != null)
        {
            settingsGroup.AddItem(
                new ApplicationMenuItem(
                    "SmartParking.Settings.Roles",
                    l["Roles"],
                    "/roles",
                    icon: Icons.Material.Filled.Man2
                ).RequirePermissions(IdentityPermissions.Roles.Default)
            );

            settingsGroup.AddItem(
                new ApplicationMenuItem(
                    "SmartParking.Settings.ContactData",
                    l["Contact Data"],
                    "/contact-data",
                    icon: Icons.Material.Filled.ContactPage
                ).RequirePermissions(ContactPermissions.Default)
            );
        }
        else
        {
            settingsGroup.AddItem(
                new ApplicationMenuItem(
                    "SmartParking.Settings.Roles",
                    l["Roles"],
                    "/roles/mdkhl",
                    icon: Icons.Material.Filled.Man2
                ).RequirePermissions(IdentityPermissions.Roles.Default)
            );
        }

        context.Menu.AddItem(settingsGroup);
    }
}
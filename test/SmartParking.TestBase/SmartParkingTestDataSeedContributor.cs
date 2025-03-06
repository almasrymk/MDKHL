using Volo.Abp.Domain.Repositories;
using SmartParking.Entities.Lookups;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using System.Collections.Generic;
using SmartParking.Enums;
using System;
using SmartParking.Entities;
using SmartParking.Entities.Subscriptions;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Entities.Shifts;
using SmartParking.Entities.Discounts;
using Volo.Abp.Identity;

namespace SmartParking;

public class SmartParkingTestDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<Region, int> _regionRepository;
    private readonly IRepository<Country, int> _countryRepository;
    private readonly IRepository<State, int> _stateRepository;
    private readonly IRepository<City, int> _cityRepository;
    private readonly IRepository<SubscriptionType, int> _subscriptionTypeRepository;
    private readonly IRepository<Subscription, int> _subscriptionRepository;
    private readonly IRepository<Organization, int> _organizationRepostory;
    private readonly IRepository<Garage, int> _garageRepostory;
    private readonly IRepository<CameraBrand, int> _cameraBrandRepository;
    private readonly IRepository<CameraModel, int> _cameraModelRepository;
    private readonly IRepository<CameraMainType, int> _cameraMainTypeRepository;
    private readonly IRepository<Camera, int> _cameraRepostory;
    private readonly IRepository<PlateColor, int> _plateColorRepository;
    private readonly IRepository<LicenceType, int> _licenceTypeRepository;
    private readonly IRepository<Shift, int> _shiftRepository;
    private readonly IRepository<KeeperShift, int> _keeperShiftRepository;
    private readonly IRepository<Discount, int> _discountRepository;
    private readonly IdentityUserManager _identityUserManager;
    public SmartParkingTestDataSeedContributor(IRepository<Region, int> regionRepository, IRepository<Country, int> countryRepository, IRepository<State, int> stateRepository, IRepository<City, int> cityRepository, IRepository<SubscriptionType, int> subscriptionTypeRepository, IRepository<Subscription, int> subscriptionRepository, IRepository<Organization, int> organizationRepostory, IRepository<Garage, int> garageRepostory, IRepository<CameraModel, int> cameraModelRepository, IRepository<CameraMainType, int> cameraMainTypeRepository, IRepository<PlateColor, int> plateColorRepository, IRepository<LicenceType, int> licenceTypeRepository, IRepository<CameraBrand, int> cameraBrandRepository, IRepository<Camera, int> cameraRepostory, IRepository<Shift, int> shiftRepository, IRepository<KeeperShift, int> keeperShiftRepository, IRepository<Discount, int> discountRepository, IdentityUserManager identityUserManager)
    {
        _regionRepository = regionRepository;
        _countryRepository = countryRepository;
        _stateRepository = stateRepository;
        _cityRepository = cityRepository;
        _subscriptionTypeRepository = subscriptionTypeRepository;
        _subscriptionRepository = subscriptionRepository;
        _organizationRepostory = organizationRepostory;
        _garageRepostory = garageRepostory;
        _cameraModelRepository = cameraModelRepository;
        _cameraMainTypeRepository = cameraMainTypeRepository;
        _plateColorRepository = plateColorRepository;
        _licenceTypeRepository = licenceTypeRepository;
        _cameraBrandRepository = cameraBrandRepository;
        _cameraRepostory = cameraRepostory;
        _shiftRepository = shiftRepository;
        _keeperShiftRepository = keeperShiftRepository;
        _discountRepository = discountRepository;
        _identityUserManager = identityUserManager;
    }
    public async Task SeedAsync(DataSeedContext context)
    {
     
        if (!await _countryRepository.AnyAsync())
        {

            var countries = new List<Country>
            {
                new Country {EnglishName = "Egypt", ArabicName = "مصر", EnglishCapital = "Cairo", ArabicCapital = "القاهرة" },
                new Country {EnglishName = "United Kingdom", ArabicName = "المملكة المتحدة", EnglishCapital = "London", ArabicCapital = "لندن" },
                new Country {EnglishName = "United States", ArabicName = "الولايات المتحدة الامريكية", EnglishCapital = "Washington", ArabicCapital = "واشنطن" },
                new Country {EnglishName = "Algeria", ArabicName = "الجزائر", EnglishCapital = "Algiers", ArabicCapital = "الجزائر العاصمة" },
            };


            await _countryRepository.InsertManyAsync(countries, autoSave: true);

            var egypt = await _countryRepository.FirstOrDefaultAsync(c => c.EnglishName == "Egypt");



            var regions = new List<Region>
            {
            new Region { ArabicName="test", EnglishName = "test",  Status = Status.Active,CountryId = egypt.Id },
            new Region { ArabicName="test", EnglishName = "test", Status = Status.Inactive,CountryId = egypt.Id},
            new Region { ArabicName="test", EnglishName = "test", Status = Status.Active,CountryId = egypt.Id},
            };


            await _regionRepository.InsertManyAsync(regions, autoSave: true);

            var test = await _regionRepository.FirstOrDefaultAsync(g => g.EnglishName == "test");

            var States = new List<State>
            {
                new State { ArabicName="test", EnglishName = "test",   RegionId = test.Id,  Status = Status.Active },
                new State { ArabicName="test", EnglishName = "test",  RegionId = test.Id, Status = Status.Active },
                new State { ArabicName="test", EnglishName = "test",  RegionId = test.Id, Status = Status.Active },
            };


            await _stateRepository.InsertManyAsync(States, autoSave: true);

            var stateTest = await _stateRepository.FirstOrDefaultAsync(g => g.EnglishName == "test");
            var cities = new List<City>
            {
                new City { ArabicName="تست", EnglishName = "test", StateId = stateTest.Id  },
                new City { ArabicName="تست", EnglishName = "test", StateId = stateTest.Id },
                new City { ArabicName="تست", EnglishName = "test", StateId = stateTest.Id},
            };


            await _cityRepository.InsertManyAsync(cities, autoSave: true);

            var cityTest = await _cityRepository.FirstOrDefaultAsync(g => g.EnglishName == "test");


            var subscriptionTypes = new List<SubscriptionType>
            {
                new SubscriptionType {TypeEn = "Monthly",TypeAr = "شهرية"},
                new SubscriptionType {TypeEn = "Quarterly",TypeAr = "ربع سنوية"},
                new SubscriptionType {TypeEn = "Yearly",TypeAr = "سنوية"},

            };


            await _subscriptionTypeRepository.InsertManyAsync(subscriptionTypes, autoSave: true);


            var subscriptionTypeTest = await _subscriptionTypeRepository.FirstOrDefaultAsync(g => g.TypeEn == "Monthly");

            var subscription = new Subscription
            {
                ArabicName = "الخطة المميزة",
                EnglishName = "Premium Plan",
                SubscriptionPrice = 49.99m,
                SubscriptionDescription = "Access to all features with no limits.",
                StatusSubscription = SubscriptionStatus.Draft,
                ActivationDate = new DateOnly(2024, 10, 1),
                InActivationDate = new DateOnly(2025, 10, 1),
                NumbersOfGarages = 5,
                NumberOfCameras = 10,
                NumberOfUsers = 100,
                NumberOfCustomers = 50,
                SubscriptionSlotNumber = 10,
                SubscriptionTypeId = subscriptionTypeTest.Id,
                ParkingSlot = 1000
            };
            var subscriptionTest = await _subscriptionRepository.InsertAsync(subscription, autoSave: true);
            if (!await _organizationRepostory.AnyAsync())
            {
                var organization = new Organization
                {
                    ArabicName = "الشركة الأولى",
                    EnglishName = "First Organization",
                    Notes = "This is a sample organization",
                    Code = "00001",
                    CommercialRegistrationNumber = "9201234511",
                    CRExpiryDate = new DateOnly(2025, 12, 31),
                    CountryId = egypt.Id,
                    RegionId = test.Id,
                    StateId = stateTest.Id,
                    CityId = cityTest.Id,
                    PhoneNum = "012345678",
                    WhatsAppNum = "+966531234567",
                    ContactMail = "contact@organization.com",
                    FirstHourPrice = 50,
                    SecondHourPrice = 50,
                    ThirdHourPrice = 50,
                    AdditionalHourPrice = 60,
                    FullDayPrice = 150,
                    IsNumberOfHoursExceeded = false,
                    NumberOfHours = 3,
                    IsClose = false,
                    ClosingHoursStart = TimeOnly.MinValue,
                    ClosingHoursEnd = TimeOnly.MaxValue,
                    Status = Status.Active,
                    SubscriptionId = subscriptionTest.Id,
                    Address = "123 Tahrir St Cairo 12345",
                    StreetName = "123 Tahrir St Cairo 12345",
                    BuildingNumber = 12,
                    PostalCode = 11145,
                    SecondaryNumber = 1163,
                    District = "حي العارض"
                };
                await _organizationRepostory.InsertAsync(organization, autoSave: true);
                var organizationTest = await _organizationRepostory.FirstOrDefaultAsync(g => g.EnglishName == "First Organization");

                var Garages = new List<Garage>
                {
                    new Garage
                    {
                      Code = "O000001-G-01",
                      NameEn = "Main Garage",
                      NameAr = "الموقف الرئيسي",
                      MaxCapacity = 150,
                      ClosingHours = new TimeOnly(22, 0),
                      OrganizationId = organizationTest.Id,
                      Status = Status.Active
                    },
                     new Garage
                    {
                        Code = "O000001-G-02",
                        NameEn = "Secondary Garage",
                        NameAr = "الموقف الثاني",
                        MaxCapacity = 100,
                        ClosingHours = new TimeOnly(20, 0),
                        OrganizationId = organizationTest.Id,
                        Status = Status.Active
                    },

                };
                await _garageRepostory.InsertManyAsync(Garages, autoSave: true);


                var garageTest = await _garageRepostory.FirstOrDefaultAsync(g => g.NameEn == "Main Garage");

                if (!await _shiftRepository.AnyAsync())
                {
                    var shiftList = new List<Shift> {

                    new Shift {
                        ArabicName = "ميد نايت " ,  EnglishName = "Mid night" ,  Notes = "test Notes" , Description = " Description test " ,
                    OrganizationId = garageTest.OrganizationId ,   GarageId = garageTest.Id ,  IsFullDay =  false , Status = 0  ,
                    StartDate = new DateOnly(2025 , 3 ,  5) ,   EndDate = new DateOnly(2025 , 3 ,  30) ,
                    StartTime =  new TimeOnly (22 , 30) ,   EndTime = new TimeOnly (2 ,  30 )
                    }  ,
                     new Shift {
                         ArabicName = "تجربه" ,  EnglishName = "Test" ,  Notes = "test Notes" , Description = " Description test " ,
                        OrganizationId = garageTest.OrganizationId ,   GarageId = garageTest.Id ,  IsFullDay =  false , Status = 0  ,
                        StartDate = new DateOnly(2025 , 3 ,  5) ,   EndDate = new DateOnly(2025 , 3 ,  29) ,
                        StartTime =  new TimeOnly (10 , 30) ,   EndTime = new TimeOnly (15 ,  30 )
                    }

                };
                    await _shiftRepository.InsertManyAsync(shiftList, autoSave: true);
                }
                IdentityUser user = new IdentityUser(new Guid("d600ff3d-c859-4327-94de-425f16ec9d55"), "keeper", "testuser1@email.com");
                await _identityUserManager.CreateAsync(user, "P@ssw0rd");

                var shift_1 = await _shiftRepository.FirstOrDefaultAsync(g => g.EnglishName == "Mid night");
                var shift_2 = await _shiftRepository.FirstOrDefaultAsync(g => g.EnglishName == "Test");

                if (!await _keeperShiftRepository.AnyAsync())
                {
                    
                    var keeperShiftList = new List<KeeperShift>
                    {
                        new KeeperShift { GarageId = shift_1.GarageId , OrganizationId = shift_1.OrganizationId , ShiftId = shift_1.Id , KeeperId = user.Id ,
                            StartDate = new DateOnly(year: 2025 ,  2 ,  27 )  , EndDate = new DateOnly(2025 , 3 , 30) , StartTime = shift_1.StartTime  , EndTime = shift_1.EndTime  } ,
                         new KeeperShift { GarageId = shift_2.GarageId , OrganizationId = shift_2.OrganizationId , ShiftId =  shift_2.Id , KeeperId =  user.Id,
                            StartDate = new DateOnly(2025 ,  2 ,  27 )  , EndDate = new DateOnly(2025 , 3 , 30) , StartTime = shift_2.StartTime  , EndTime = shift_2.EndTime  } ,
                          new KeeperShift { GarageId = shift_2.GarageId , OrganizationId = shift_2.OrganizationId , ShiftId =  shift_2.Id , KeeperId =  user.Id,
                            StartDate = new DateOnly(2025 ,  2 ,  27 )  , EndDate = new DateOnly(2025 , 3 , 1) , StartTime = shift_2.StartTime  , EndTime = shift_2.EndTime  } ,

                    };
                    await _keeperShiftRepository.InsertManyAsync(keeperShiftList , true);

                }

             
                var cameraBrand = new CameraBrand { ArabicName = "تست", EnglishName = "test" };
                await _cameraBrandRepository.InsertAsync(cameraBrand, autoSave: true);
                var cameraBrandTest = await _cameraBrandRepository.FirstOrDefaultAsync(_ => _.EnglishName == "test");

                var cameraModel = new CameraModel
                {
                    EnglishName = "test",
                    ArabicName = "تست",
                    CameraBrand = cameraBrandTest,
                };
                await _cameraModelRepository.InsertAsync(cameraModel, autoSave: true);
                var cameraModelTest = await _cameraModelRepository.FirstOrDefaultAsync(_ => _.EnglishName == "test");

                var cameraMainType = new CameraMainType
                {
                    EnglishName = "test",
                    ArabicName = "تست",
                    CameraBrand = cameraBrandTest,
                };
                await _cameraMainTypeRepository.InsertAsync(cameraMainType, autoSave: true);
                var cameraMainTypeTest = await _cameraMainTypeRepository.FirstOrDefaultAsync(_ => _.EnglishName == "test");
                var cameras = new List<Camera>{
                new Camera
                {
                    GarageId = garageTest.Id,    // Link camera to the found Garage
                    Name = "Entry",         // Provide valid camera name
                    Model = cameraModelTest,           // Add the model of the camera
                    MainType = cameraMainTypeTest,         // Add the main type
                    BrandName = cameraBrandTest,       // Add the brand name
                    DeviceId = "233f1b26-9350-eb14-03e7-78934a049350",             // Add a unique code
                    CameraIP = "192.168.1.10",   // Add camera IP address
                    MacAddress = "00:11:22:33:44:55", // Add MAC address
                    CameraType = CameraType.Entry,            // Set camera type
                    Status = Status.Active,      // Set the status of the camera
                    TenantId = null,              // Set TenantId if multi-tenant, or null
                    OrganizationId = organizationTest.Id,

                },
                 new Camera
                 {
                     GarageId = garageTest.Id,    // Link camera to the found Garage
                     Name = "Exit",         // Provide valid camera name
                     Model = cameraModelTest,           // Add the model of the camera
                    MainType = cameraMainTypeTest,         // Add the main type
                    BrandName = cameraBrandTest,       // Add the brand name
                     DeviceId = "df9a5b33-adf7-f454-49dd-d9624605adf7",             // Add a unique code
                     CameraIP = "192.168.1.11",   // Add camera IP address
                     MacAddress = "00:11:22:33:44:55", // Add MAC address
                     CameraType = CameraType.Exit,            // Set camera type
                     Status = Status.Active,      // Set the status of the camera
                     TenantId = null,              // Set TenantId if multi-tenant, or null
                     OrganizationId = organizationTest.Id,
                 }
                };


                await _cameraRepostory.InsertManyAsync(cameras,
                      autoSave: true
                  );
            }

        }

        if (!await _plateColorRepository.AnyAsync())
        {


            var plateColors = new List<PlateColor>
               {
                   new PlateColor {  EnglishName = "Red", ArabicName = "أحمر" },
                   new PlateColor {  EnglishName = "Blue", ArabicName = "أزرق" },
                   new PlateColor {  EnglishName = "Green", ArabicName = "أخضر" },
                   new PlateColor {  EnglishName = "Yellow", ArabicName = "أصفر" },
                   new PlateColor {  EnglishName = "White", ArabicName = "أبيض" },
                   new PlateColor {  EnglishName = "Silver", ArabicName = "فضى" }
               };

            await _plateColorRepository.InsertManyAsync(plateColors, autoSave: true);

            var redplate = await _plateColorRepository.FirstOrDefaultAsync(g => g.EnglishName == "Red");
            var blueplate = await _plateColorRepository.FirstOrDefaultAsync(g => g.EnglishName == "Blue");
            var whiteplate = await _plateColorRepository.FirstOrDefaultAsync(g => g.EnglishName == "White");
            var yellowplate = await _plateColorRepository.FirstOrDefaultAsync(g => g.EnglishName == "Yellow");
            var silverplate = await _plateColorRepository.FirstOrDefaultAsync(g => g.EnglishName == "Yellow");
            var greenplate = await _plateColorRepository.FirstOrDefaultAsync(g => g.EnglishName == "Green");


            var licenceTypes = new List<LicenceType>
                {
                // Two licence types sharing the same PlateColorId (e.g., Red with Id = 1)
                new LicenceType {  EnglishName = "Private Car", ArabicName = "خاص", PlateColorId = whiteplate.Id },
                new LicenceType {  EnglishName = "Public Transport", ArabicName = "نقل عام", PlateColorId = yellowplate.Id },
                new LicenceType {  EnglishName = "Private Transport", ArabicName = "نقل خاص", PlateColorId = blueplate.Id },
                new LicenceType {  EnglishName = "Public Bus", ArabicName = "حافلة صغيرة عامة", PlateColorId = yellowplate.Id },
                new LicenceType {  EnglishName = "Private Bus", ArabicName = "حافلة صغيرة خاصة", PlateColorId = blueplate.Id },
                new LicenceType {  EnglishName = "Taxi", ArabicName = "اجرة", PlateColorId = yellowplate.Id },
                new LicenceType {  EnglishName = "Equipment", ArabicName = "معدات ثقيلة", PlateColorId = blueplate.Id },
                new LicenceType {  EnglishName = "Export", ArabicName = "تصدير", PlateColorId = silverplate.Id },
                new LicenceType {  EnglishName = "Motorcycle", ArabicName = "دراجة نارية", PlateColorId = whiteplate.Id },
                new LicenceType {  EnglishName = "Temp", ArabicName = "مؤقت", PlateColorId = silverplate.Id }, 

                // Unique licence types with their own PlateColorId
                new LicenceType {  EnglishName = "Antique", ArabicName = "اقتناء تاريخي", PlateColorId = whiteplate.Id },
                new LicenceType {  EnglishName = "Diplomatic", ArabicName = "دبلوماسي", PlateColorId = greenplate.Id }
                };

            await _licenceTypeRepository.InsertManyAsync(licenceTypes, autoSave: true);


        }

    }
}

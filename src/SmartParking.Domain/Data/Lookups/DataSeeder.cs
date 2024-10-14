using SmartParking.Entities;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Lookups;
using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Subscriptions;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Data.Lookups;

internal class DataSeeder : IDataSeedContributor, ITransientDependency
{


    private readonly IRepository<Country, int> _countryRepository;
    private readonly IRepository<Governorate, int> _governorateRepository;
    private readonly IRepository<City, int> _cityRepository;
    private readonly IRepository<Region, int> _regionRepository;
    private readonly IRepository<State, int> _stateRepository;
    private readonly IRepository<PlateColor, int> _plateColorRepository;
    private readonly IRepository<LicenceType, int> _licenceTypeRepository;
    private readonly IRepository<Organization, int> _organizationRepostory;
    private readonly IRepository<Garage, int> _garageRepostory;
    private readonly IRepository<Camera, int> _cameraRepostory;
    private readonly IRepository<Subscription, int> _subscriptionRepository;
    private readonly IRepository<SubscriptionType, int> _subscriptionTypeRepository;

    public DataSeeder(
        IRepository<Country, int> countryRepository,
        IRepository<Governorate, int> governorateRepository,
        IRepository<City, int> cityRepository,
        IRepository<Region, int> regionRepository,
        IRepository<State, int> stateRepository,
        IRepository<PlateColor, int> plateColorRepository,
        IRepository<LicenceType, int> licenceTypeRepository,
        IRepository<Organization,int> organization,
        IRepository<Garage, int> garageRepostory,
        IRepository<Camera, int> cameraRepostory,
        IRepository<Subscription,int> subscriptionRepository,
        IRepository<SubscriptionType, int> subscriptionTypeRepository)

    {
        _countryRepository = countryRepository;
        _governorateRepository = governorateRepository;
        _cityRepository = cityRepository;
        _regionRepository = regionRepository;
        _stateRepository = stateRepository;
        _licenceTypeRepository = licenceTypeRepository;
        _organizationRepostory = organization;
        _plateColorRepository = plateColorRepository;
        _garageRepostory = garageRepostory;
        _cameraRepostory = cameraRepostory;
        _subscriptionRepository = subscriptionRepository;
        _subscriptionTypeRepository = subscriptionTypeRepository;
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

            foreach (var country in countries)
            {
                await _countryRepository.InsertAsync(country, autoSave: true);
            }
            var egypt = await _countryRepository.FirstOrDefaultAsync(c => c.EnglishName == "Egypt");
            var governorates = new List<Governorate>
            {
               new Governorate {  EnglishName = "Alexandria", ArabicName = "الإسكندرية", EnglishCapital = "Alexandria", ArabicCapital = "الإسكندرية", CountryId = egypt.Id },
                 new Governorate {  EnglishName = "Ismailia", ArabicName = "الإسماعيلية", EnglishCapital = "Ismailia", ArabicCapital = "الإسماعيلية", CountryId = egypt.Id },
                 new Governorate {  EnglishName = "Aswan", ArabicName = "أسوان", EnglishCapital = "Aswan", ArabicCapital = "أسوان", CountryId = egypt.Id },
                 new Governorate {  EnglishName = "Asyut", ArabicName = "أسيوط", EnglishCapital = "Asyut", ArabicCapital = "أسيوط", CountryId = egypt.Id },
                 new Governorate {  EnglishName = "Luxor", ArabicName = "الأقصر", EnglishCapital = "Luxor", ArabicCapital = "الأقصر", CountryId = egypt.Id },
                 new Governorate { EnglishName = "Red Sea", ArabicName = "البحر الأحمر", EnglishCapital = "Hurghada", ArabicCapital = "الغردقة", CountryId = egypt.Id },
            };

            foreach (var governorate in governorates)
            {
                await _governorateRepository.InsertAsync(governorate, autoSave: true);
            }
            var alx = await _governorateRepository.FirstOrDefaultAsync(g => g.EnglishName == "Alexandria");
            var cities = new List<City>
            {
                new City { ArabicName="test", EnglishName = "test", GovernorateId = alx.Id },
                new City { ArabicName="test", EnglishName = "test", GovernorateId = alx.Id },
                new City {  ArabicName="test", EnglishName = "test", GovernorateId = alx.Id },
            };

            foreach (var city in cities)
            {
                await _cityRepository.InsertAsync(city, autoSave: true);
            }
            var cityTest = await _cityRepository.FirstOrDefaultAsync(g => g.EnglishName == "test");
            var regions = new List<Region>
            {
            new Region { ArabicName="test", EnglishName = "test",  Status = Status.Active,CountryId = egypt.Id },
            new Region { ArabicName="test", EnglishName = "test", Status = Status.Inactive,CountryId = egypt.Id},
            new Region { ArabicName="test", EnglishName = "test", Status = Status.Active,CountryId = egypt.Id},
            };

            foreach (var region in regions)
            {
                await _regionRepository.InsertAsync(region, autoSave: true);
            }
            var test = await _regionRepository.FirstOrDefaultAsync(g => g.EnglishName == "test");
            var States = new List<State>
            {
                new State { ArabicName="test", EnglishName = "test",   RegionId = test.Id,  Status = Status.Active },
                new State { ArabicName="test", EnglishName = "test",  RegionId = test.Id, Status = Status.Active },
                new State { ArabicName="test", EnglishName = "test",  RegionId = test.Id, Status = Status.Active },
            };

            foreach (var State in States)
            {
                await _stateRepository.InsertAsync(State, autoSave: true);
            }
            var stateTest = await _stateRepository.FirstOrDefaultAsync(g => g.EnglishName == "test");
            var subscriptionTypes = new List<SubscriptionType>
        {
            new SubscriptionType {Type = "Monthly"},
            new SubscriptionType {Type = "Quarterly"},
            new SubscriptionType {Type = "Yearly"},
        };

            foreach (var subscriptionType in subscriptionTypes)
            {
                await _subscriptionTypeRepository.InsertAsync(subscriptionType, autoSave: true);
            }

            var subscriptionTypeTest = await _subscriptionTypeRepository.FirstOrDefaultAsync(g => g.Type == "Monthly");

            var subscription = new Subscription
            {
                SubscriptionName = "Premium Plan",
                SubscriptionPrice = 49.99m,
                SubscriptionDescription = "Access to all features with no limits.",
                Status = SubscriptionStatus.Draft,
                ActivationDate = new DateOnly(2024, 10, 1),
                InActivationDate = new DateOnly(2025, 10, 1),
                NumbersOfGarages = 5,
                NumberOfCameras = 10,
                NumberOfCustomers = 100,
                NumberOfMobileCustomers = 50,
                NumberOfOrganizations = 50,
                SubscriptionSlotNumber = 10,
                SubscriptionTypeId = subscriptionTypeTest.Id,

            };
            var subscriptionTest = await _subscriptionRepository.InsertAsync(subscription, autoSave: true);

            var organization = new Organization
            {
                ArabicName = "الشركة الأولى",
                EnglishName = "First Organization",
                Notes = "This is a sample organization",
                Code = "00001",
                UnifiedLicenseNum = "920123451",
                CRExpiryDate = new DateOnly(2025, 12, 31),
                LicenseExpiryDate = new DateOnly(2026, 12, 31),
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
                FullDay = 150,
                IsNumberOfHoursExceeded = false,
                NumberOfHours = 3,
                IsFullDayExceeded = false,  
                ExceededTime = TimeOnly.MinValue,
                Status = Status.Active,
                SubscriptionId= subscriptionTest.Id,
            };
            await _organizationRepostory.InsertAsync(organization,autoSave:true);
            var organizationTest = await _organizationRepostory.FirstOrDefaultAsync(g => g.EnglishName == "First Organization");


            await _garageRepostory.InsertAsync(
                new Garage
                {
                  Code = "O000001-G-01",
                  Name = "Main Garage",
                  MaxCapacity = 150,
                  ClosingHours = new TimeOnly(22, 0), 
                  OrganizationId = organizationTest.Id,
                  Status = Status.Active
                },
                autoSave: true
            );

            await _garageRepostory.InsertAsync(
                new Garage
                {
                    Code = "O000001-G-02",
                    Name = "Secondary Garage",
                    MaxCapacity = 100,
                    ClosingHours = new TimeOnly(20, 0), 
                    OrganizationId = organizationTest.Id,
                    Status = Status.Active
                },
                autoSave: true
            );

            var garageTest = await _garageRepostory.FirstOrDefaultAsync(g => g.Name == "Main Garage");


            await _cameraRepostory.InsertAsync(
                new Camera
                {
                    GarageId = garageTest.Id,    // Link camera to the found Garage
                    Name = "Camera One",         // Provide valid camera name
                    Model = "Model X",           // Add the model of the camera
                    MainType = "Type A",         // Add the main type
                    BrandName = "Brand Z",       // Add the brand name
                    Code = "CAM001",             // Add a unique code
                    CameraIP = "192.168.1.10",   // Add camera IP address
                    MacAddress = "00:11:22:33:44:55", // Add MAC address
                    CameraType = CameraType.Exit,            // Set camera type
                    Status = Status.Active,      // Set the status of the camera
                    TenantId = null              // Set TenantId if multi-tenant, or null
                },
                autoSave: true
            );

            await _cameraRepostory.InsertAsync(
                new Camera
                {
                    GarageId = garageTest.Id,    // Link camera to the found Garage
                    Name = "Camera Two",         // Provide valid camera name
                    Model = "Model X",           // Add the model of the camera
                    MainType = "Type A",         // Add the main type
                    BrandName = "Brand Z",       // Add the brand name
                    Code = "CAM001",             // Add a unique code
                    CameraIP = "192.168.1.11",   // Add camera IP address
                    MacAddress = "00:11:22:33:44:55", // Add MAC address
                    CameraType = CameraType.Exit,            // Set camera type
                    Status = Status.Active,      // Set the status of the camera
                    TenantId = null              // Set TenantId if multi-tenant, or null
                },
                autoSave: true
            );

        }

        if (!await _plateColorRepository.AnyAsync())
        {
            if (!await _plateColorRepository.AnyAsync())
            {

                var plateColors = new List<PlateColor>
        {
            new PlateColor {  EnglishName = "Red", ArabicName = "أحمر" },
            new PlateColor {  EnglishName = "Blue", ArabicName = "أزرق" },
            new PlateColor {  EnglishName = "Green", ArabicName = "أخضر" },
            new PlateColor {  EnglishName = "Yellow", ArabicName = "أصفر" },
            new PlateColor {  EnglishName = "White", ArabicName = "أبيض" }
        };

                foreach (var plateColor in plateColors)
                {
                    await _plateColorRepository.InsertAsync(plateColor, autoSave: true);
                }
                var redplate = await _plateColorRepository.FirstOrDefaultAsync(g => g.EnglishName == "Red");
                var blueplate = await _plateColorRepository.FirstOrDefaultAsync(g => g.EnglishName == "Blue");


                var licenceTypes = new List<LicenceType>
        {
                // Two licence types sharing the same PlateColorId (e.g., Red with Id = 1)
                new LicenceType {  EnglishName = "Personal", ArabicName = "شخصي", PlateColorId = redplate.Id },
            new LicenceType {  EnglishName = "Commercial", ArabicName = "تجاري", PlateColorId = redplate.Id }, 

                // Unique licence types with their own PlateColorId
                new LicenceType {  EnglishName = "Government", ArabicName = "حكومي", PlateColorId = blueplate.Id }, // Blue
                new LicenceType {  EnglishName = "Diplomatic", ArabicName = "دبلوماسي", PlateColorId = blueplate.Id }, // Blue
                new LicenceType {  EnglishName = "Military", ArabicName = "عسكري", PlateColorId = blueplate.Id}  // Blue
            };
                foreach (var licenceType in licenceTypes)
                {
                    await _licenceTypeRepository.InsertAsync(licenceType, autoSave: true);
                }
            }
        }
    }
}

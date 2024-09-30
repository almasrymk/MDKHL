using SmartParking.Entities;
using SmartParking.Entities.Cameras;
using SmartParking.Entities.Lookups;
using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Data.Lookups
{
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

        public DataSeeder(
            IRepository<Country, int> countryRepository,
            IRepository<Governorate, int> governorateRepository,
            IRepository<City, int> cityRepository,
            IRepository<Region, int> regionRepository,
            IRepository<State, int> stateRepository,
            IRepository<PlateColor, int> plateColorRepository,
            IRepository<LicenceType, int> licenceTypeRepository,
            IRepository<Organization,int> organization)
        {
            _countryRepository = countryRepository;
            _governorateRepository = governorateRepository;
            _cityRepository = cityRepository;
            _regionRepository = regionRepository;
            _stateRepository = stateRepository;
            _licenceTypeRepository = licenceTypeRepository;
            _organizationRepostory = organization;
            _plateColorRepository = plateColorRepository;
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
                    Status = Status.Active,
                };
                await _organizationRepostory.InsertAsync(organization,autoSave:true);
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
}
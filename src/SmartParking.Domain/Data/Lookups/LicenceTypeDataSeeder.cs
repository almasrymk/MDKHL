using SmartParking.Entities.Lookups.VehiclesLookups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Data.Lookups;

public class LicenceTypeDataSeeder : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<LicenceType, int> _licenceTypeRepository;

    public LicenceTypeDataSeeder(IRepository<LicenceType, int> licenceTypeRepository)
    {
        _licenceTypeRepository = licenceTypeRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        // Check if LicenceType data already exists
        if (!await _licenceTypeRepository.AnyAsync())
        {
            // Define the list of licence types to seed, with two types sharing the same PlateColorId
            var licenceTypes = new List<LicenceType>
            {
                    // Two licence types sharing the same PlateColorId (e.g., Red with Id = 1)
                    new LicenceType {  EnglishName = "Personal", ArabicName = "شخصي", PlateColorId = 1 },
                new LicenceType {  EnglishName = "Commercial", ArabicName = "تجاري", PlateColorId = 1 }, 

                    // Unique licence types with their own PlateColorId
                    new LicenceType {  EnglishName = "Government", ArabicName = "حكومي", PlateColorId = 2 }, // Blue
                    new LicenceType {  EnglishName = "Diplomatic", ArabicName = "دبلوماسي", PlateColorId = 3 }, // Green
                    new LicenceType {  EnglishName = "Military", ArabicName = "عسكري", PlateColorId = 4 }  // Yellow
                };

            // Insert the licence types into the repository
            await _licenceTypeRepository.InsertManyAsync(licenceTypes);
        }
    }
}

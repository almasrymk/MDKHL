using SmartParking.Entities.Lookups;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Data.Lookups;

public class RegionDataSeeder : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<Region, int> _regionRepository;

    public RegionDataSeeder(IRepository<Region, int> regionRepository)
    {
        _regionRepository = regionRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        if (!await _regionRepository.AnyAsync())
        {
            var emaraat = new List<Region>
            {
                new Region { ArabicName="test", EnglishName = "test",  Status = Status.Active,CountryId = 1 },
                new Region { ArabicName="test", EnglishName = "test", Status = Status.Inactive,CountryId = 2},
                new Region { ArabicName="test", EnglishName = "test", Status = Status.Active,CountryId = 3},


            };
            foreach (var emaraa in emaraat)
            {
                await _regionRepository.InsertAsync(emaraa);
            }
        }
    }
}

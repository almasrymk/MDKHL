using SmartParking.Entities.Lookups;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Data.Lookups;

//public class EmaraaDataSeeder : IDataSeedContributor, ITransientDependency
//{
//    private readonly IRepository<Emaraa, int> _emaraaRepository;

//    public EmaraaDataSeeder(IRepository<Emaraa, int> emaraaRepository)
//    {
//        _emaraaRepository = emaraaRepository;
//    }

    public async Task SeedAsync(DataSeedContext context)
    {
        if (!await _emaraaRepository.AnyAsync())
        {
            var emaraat = new List<Emaraa>
            {
                new Emaraa {  ArabicName="test", EnglishName = "test",  Status = Status.Active,CountryId = 1 },
                new Emaraa { ArabicName="test", EnglishName = "test", Status = Status.Inactive,CountryId = 2},
                new Emaraa { ArabicName="test", EnglishName = "test", Status = Status.Active,CountryId = 3},

        
            };
            foreach (var emaraa in emaraat)
            {
                await _emaraaRepository.InsertAsync(emaraa);
            }
        }
    }
}

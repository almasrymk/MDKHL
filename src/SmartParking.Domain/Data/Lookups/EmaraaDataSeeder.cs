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
                new Emaraa {Id = 1, CountryId = 1,  ArabicName="test", EnglishName = "test",  Status = Status.Active },
                new Emaraa {Id = 2, CountryId = 2, ArabicName="test", EnglishName = "test", Status = Status.Inactive},
                new Emaraa {Id = 3, CountryId = 3, ArabicName="test", EnglishName = "test", Status = Status.Active},

        
            };

            foreach (var emaraa in emaraat)
            {
                await _emaraaRepository.InsertAsync(emaraa);
            }
        }
    }
}

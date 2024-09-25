using SmartParking.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Data.Lookups
{
    public class CityDataSeeder : IDataSeedContributor, ITransientDependency
    {
        private readonly IRepository<City, int> _cityRepository;

        public CityDataSeeder(IRepository<City, int> cityRepository)
        {
            _cityRepository = cityRepository;
        }

        public async Task SeedAsync(DataSeedContext context)
        {
            if (!await _cityRepository.AnyAsync())
            {
                var cities = new List<City>
                {
                    new City { ArabicName="test", EnglishName = "test", GovernorateId = 1 },
                    new City { ArabicName="test", EnglishName = "test", GovernorateId = 2 },
                    new City {  ArabicName="test", EnglishName = "test", GovernorateId = 3 },


                };

                foreach (var city in cities)
                {
                    await _cityRepository.InsertAsync(city);
                }
            }
        }
    }
}

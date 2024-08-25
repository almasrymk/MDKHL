using SmartParking.Entities.Lookups;
using SmartParking.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Data.Lookups
{
    public class StatsDataSeeder : IDataSeedContributor, ITransientDependency
    {
        private readonly IRepository<Stats, int> _statsRepository;

        public StatsDataSeeder(IRepository<Stats, int> statsRepository)
        {
            _statsRepository = statsRepository;
        }

        public async Task SeedAsync(DataSeedContext context)
        {
            if (!await _statsRepository.AnyAsync())
            {
                var statsList = new List<Stats>
                {
                    new Stats { Id = 1, EmaraaId = 1, Status = Status.Active },
                    new Stats { Id = 2, EmaraaId = 2, Status = Status.Active },
                    new Stats { Id = 3, EmaraaId = 3, Status = Status.Active },

                };

                foreach (var stats in statsList)
                {
                    await _statsRepository.InsertAsync(stats);
                }
            }
        }
    }
}

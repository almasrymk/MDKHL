using SmartParking.Entities.Lookups;
using SmartParking.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Data.Lookups;

public class StateDataSeeder : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<State, int> _StateRepository;

    public async Task SeedAsync(DataSeedContext context)
    {
        if (!await _StateRepository.AnyAsync())
        {
            var StateList = new List<State>
                {
                    new State { ArabicName="test", EnglishName = "test", EmaraaId = 1, Status = Status.Active },
                    new State { ArabicName="test", EnglishName = "test", EmaraaId = 2, Status = Status.Active },
                    new State { ArabicName="test", EnglishName = "test", EmaraaId = 3, Status = Status.Active },
            };

            foreach (var State in StateList)
            {
                await _StateRepository.InsertAsync(State);
            }
        }
    }

}

//using SmartParking.Entities.Lookups;
//using SmartParking.Enums;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Volo.Abp.Data;
//using Volo.Abp.DependencyInjection;
//using Volo.Abp.Domain.Repositories;

//namespace SmartParking.Data.Lookups;

//public class StateDataSeeder : IDataSeedContributor, ITransientDependency
//{
//    private readonly IRepository<State, int> _stateRepository;

//    public StateDataSeeder(IRepository<State, int> stateRepository)
//    {
//        _stateRepository = stateRepository;
//    }
//    public async Task SeedAsync(DataSeedContext context)
//    {
//        if (!await _stateRepository.AnyAsync())
//        {
//            var StateList = new List<State>
//                {
//                    new State { ArabicName="test", EnglishName = "test", RegionId = 1, Status = Status.Active },
//                    new State { ArabicName="test", EnglishName = "test", RegionId = 2, Status = Status.Active },
//                    new State { ArabicName="test", EnglishName = "test", RegionId = 3, Status = Status.Active },
//            };

//            foreach (var State in StateList)
//            {
//                await _stateRepository.InsertAsync(State);
//            }
//        }
//    }

//}

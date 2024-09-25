using SmartParking.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Data.Lookups;

//public class UserTypeDataSeeder : IDataSeedContributor, ITransientDependency
//{
//    private readonly IRepository<UserType, int> _userTypeRepository;

        public async Task SeedAsync(DataSeedContext context)
        {
            if (!await _userTypeRepository.AnyAsync())
            {
                var userTypes = new List<UserType>
                {
                    new UserType { Id=1,  EnglishName = "Smart Admin", ArabicName = " مدبر النظام" , },
                    new UserType { Id=2, EnglishName = "Hotel Admin", ArabicName = "مدير الفندق" },
                    new UserType { Id=3, EnglishName = "Customer", ArabicName = "عميل"}
                };
             
                await this._userTypeRepository.InsertManyAsync(userTypes);
            }
        }
    }
}

using SmartParking.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Data.Lookups;

public class UserTypeDataSeeder : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<UserType, int> _userTypeRepository;
    public UserTypeDataSeeder(IRepository<UserType, int> userTypeRepository)
    {
        _userTypeRepository = userTypeRepository;
    }
    public async Task SeedAsync(DataSeedContext context)
    {
        if (!await _userTypeRepository.AnyAsync())
        {
            var userTypes = new List<UserType>
                {
                    new UserType { EnglishName = "Smart Admin", ArabicName = " مدبر النظام" , },
                    new UserType { EnglishName = "Hotel Admin", ArabicName = "مدير الفندق" },
                    new UserType { EnglishName = "Customer", ArabicName = "عميل"},
                    new UserType {EnglishName = "Keeper",ArabicName = "الحارس"}
                };

            await this._userTypeRepository.InsertManyAsync(userTypes);
        }
    }
}


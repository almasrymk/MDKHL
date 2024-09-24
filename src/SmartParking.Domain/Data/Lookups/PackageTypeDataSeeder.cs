using SmartParking.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Data
{
    public class PackageTypeDataSeeder : IDataSeedContributor, ITransientDependency
    {
        private readonly IRepository<PackageType, int> _PackageTypeeRepository;

        public PackageTypeDataSeeder(IRepository<PackageType, int> PackageTypeeRepository)
        {
            this._PackageTypeeRepository = PackageTypeeRepository;
        }

        public async Task SeedAsync(DataSeedContext context)
        {
            if (!await _PackageTypeeRepository.AnyAsync())
            {
                var PackageTypes = new List<PackageType>
                {
                    new PackageType { Id=1, EnglishName = "Daily", ArabicName = "يومى" , },
                    new PackageType { Id=2, EnglishName = "Weekly", ArabicName = "اسبوعى" },
                    new PackageType { Id=3, EnglishName = "Monthly", ArabicName = "شهرى"},
                    new PackageType { Id=4, EnglishName = "Quarterly", ArabicName = "ربع سنوى"},
                    new PackageType { Id=5, EnglishName = "Yearly", ArabicName = "سنوى"}

                };
             
                await this._PackageTypeeRepository.InsertManyAsync(PackageTypes);
            }
        }
    }
}

//using SmartParking.Entities;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Volo.Abp.Data;
//using Volo.Abp.DependencyInjection;
//using Volo.Abp.Domain.Repositories;

//namespace SmartParking.Data
//{
//    public class PackageTypeDataSeeder : IDataSeedContributor, ITransientDependency
//    {
//        private readonly IRepository<PackageType, int> _PackageTypeeRepository;

//        public PackageTypeDataSeeder(IRepository<PackageType, int> PackageTypeeRepository)
//        {
//            this._PackageTypeeRepository = PackageTypeeRepository;
//        }

//        public async Task SeedAsync(DataSeedContext context)
//        {
//            if (!await _PackageTypeeRepository.AnyAsync())
//            {
//                var PackageTypes = new List<PackageType>
//                {
//                    new PackageType {  EnglishName = "Daily", ArabicName = "يومى" , },
//                    new PackageType {  EnglishName = "Weekly", ArabicName = "اسبوعى" },
//                    new PackageType {  EnglishName = "Monthly", ArabicName = "شهرى"},
//                    new PackageType {  EnglishName = "Quarterly", ArabicName = "ربع سنوى"},
//                    new PackageType {  EnglishName = "Yearly", ArabicName = "سنوى"}

//                };

//                await this._PackageTypeeRepository.InsertManyAsync(PackageTypes);
//            }
//        }
//    }
//}

using SmartParking.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Data.Lookups
{
    public class GovernoratesDataSeeder : IDataSeedContributor, ITransientDependency
    {
        private readonly IRepository<Governorate, int> _governorateRepository;

        public GovernoratesDataSeeder(IRepository<Governorate, int> governorateRepository)
        {
            this._governorateRepository = governorateRepository;
        }

        public async Task SeedAsync(DataSeedContext context)
        {
            if (!await _governorateRepository.AnyAsync())
            {
                var governorates = new List<Governorate>
                {
                     new Governorate { Id = 1, EnglishName = "Alexandria", ArabicName = "الإسكندرية", EnglishCapital = "Alexandria", ArabicCapital = "الإسكندرية", CountryId = 53 },
                     new Governorate { Id = 2, EnglishName = "Ismailia", ArabicName = "الإسماعيلية", EnglishCapital = "Ismailia", ArabicCapital = "الإسماعيلية", CountryId = 53 },
                     new Governorate { Id = 3, EnglishName = "Aswan", ArabicName = "أسوان", EnglishCapital = "Aswan", ArabicCapital = "أسوان", CountryId = 53 },
                     new Governorate { Id = 4, EnglishName = "Asyut", ArabicName = "أسيوط", EnglishCapital = "Asyut", ArabicCapital = "أسيوط", CountryId = 53 },
                     new Governorate { Id = 5, EnglishName = "Luxor", ArabicName = "الأقصر", EnglishCapital = "Luxor", ArabicCapital = "الأقصر", CountryId = 53 },
                     new Governorate { Id = 6, EnglishName = "Red Sea", ArabicName = "البحر الأحمر", EnglishCapital = "Hurghada", ArabicCapital = "الغردقة", CountryId = 53 },
                     new Governorate { Id = 7, EnglishName = "Beheira", ArabicName = "البحيرة", EnglishCapital = "Damanhur", ArabicCapital = "دمنهور", CountryId = 53 },
                     new Governorate { Id = 8, EnglishName = "Beni Suef", ArabicName = "بني سويف", EnglishCapital = "Beni Suef", ArabicCapital = "بني سويف", CountryId = 53 },
                     new Governorate { Id = 9, EnglishName = "Port Said", ArabicName = "بورسعيد", EnglishCapital = "Port Said", ArabicCapital = "بورسعيد", CountryId = 53 },
                     new Governorate { Id = 10, EnglishName = "South Sinai", ArabicName = "جنوب سيناء", EnglishCapital = "El Tor", ArabicCapital = "الطور", CountryId = 53 },
                     new Governorate { Id = 11, EnglishName = "Giza", ArabicName = "الجيزة", EnglishCapital = "Giza", ArabicCapital = "الجيزة", CountryId = 53 },
                     new Governorate { Id = 12, EnglishName = "Dakahlia", ArabicName = "الدقهلية", EnglishCapital = "Mansoura", ArabicCapital = "المنصورة", CountryId = 53 },
                     new Governorate { Id = 13, EnglishName = "Damietta", ArabicName = "دمياط", EnglishCapital = "Damietta", ArabicCapital = "دمياط", CountryId = 53 },
                     new Governorate { Id = 14, EnglishName = "Sohag", ArabicName = "سوهاج", EnglishCapital = "Sohag", ArabicCapital = "سوهاج", CountryId = 53 },
                     new Governorate { Id = 15, EnglishName = "Suez", ArabicName = "السويس", EnglishCapital = "Suez", ArabicCapital = "السويس", CountryId = 53 },
                     new Governorate { Id = 16, EnglishName = "Sharqia", ArabicName = "الشرقية", EnglishCapital = "Zagazig", ArabicCapital = "الزقازيق", CountryId = 53 },
                     new Governorate { Id = 17, EnglishName = "North Sinai", ArabicName = "شمال سيناء", EnglishCapital = "Arish", ArabicCapital = "العريش", CountryId = 53 },
                     new Governorate { Id = 18, EnglishName = "Gharbia", ArabicName = "الغربية", EnglishCapital = "Tanta", ArabicCapital = "طنطا", CountryId = 53 },
                     new Governorate { Id = 19, EnglishName = "Faiyum", ArabicName = "الفيوم", EnglishCapital = "Faiyum", ArabicCapital = "الفيوم", CountryId = 53 },
                     new Governorate { Id = 20, EnglishName = "Cairo", ArabicName = "القاهرة", EnglishCapital = "Cairo", ArabicCapital = "القاهرة", CountryId = 53 },
                     new Governorate { Id = 21, EnglishName = "Qalyubia", ArabicName = "القليوبية", EnglishCapital = "Banha", ArabicCapital = "بنها", CountryId = 53 },
                     new Governorate { Id = 22, EnglishName = "Qina", ArabicName = "قنا", EnglishCapital = "Qina", ArabicCapital = "قنا", CountryId = 53 },
                     new Governorate { Id = 23, EnglishName = "Kafr El Sheikh", ArabicName = "كفر الشيخ", EnglishCapital = "Kafr El Sheikh", ArabicCapital = "كفر الشيخ", CountryId = 53 },
                     new Governorate { Id = 24, EnglishName = "Matruh", ArabicName = "مطروح", EnglishCapital = "Marsa Matruh", ArabicCapital = "مرسى مطروح", CountryId = 53 },
                     new Governorate { Id = 25, EnglishName = "Monufia", ArabicName = "المنوفية", EnglishCapital = "Shibin El Kom", ArabicCapital = "شبين الكوم", CountryId = 53 },
                     new Governorate { Id = 26, EnglishName = "Minya", ArabicName = "المنيا", EnglishCapital = "Minya", ArabicCapital = "المنيا", CountryId = 53 },
                     new Governorate { Id = 27, EnglishName = "New Valley", ArabicName = "الوادي الجديد", EnglishCapital = "Kharga", ArabicCapital = "الخارجة", CountryId = 53 }
                };
                await this._governorateRepository.InsertManyAsync(governorates);
            }
        }
    }
}

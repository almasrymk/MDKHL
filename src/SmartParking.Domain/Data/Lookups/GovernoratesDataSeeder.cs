using SmartParking.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Data.Lookups;

public class GovernoratesDataSeeder : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<Governorate, int> _governorateRepository;

    public async Task SeedAsync(DataSeedContext context)
        {
            if (!await _governorateRepository.AnyAsync())
            {
                var governorates = new List<Governorate>
                {
                     new Governorate {  EnglishName = "Alexandria", ArabicName = "الإسكندرية", EnglishCapital = "Alexandria", ArabicCapital = "الإسكندرية", CountryId = 53 },
                     new Governorate {  EnglishName = "Ismailia", ArabicName = "الإسماعيلية", EnglishCapital = "Ismailia", ArabicCapital = "الإسماعيلية", CountryId = 53 },
                     new Governorate {  EnglishName = "Aswan", ArabicName = "أسوان", EnglishCapital = "Aswan", ArabicCapital = "أسوان", CountryId = 53 },
                     new Governorate {  EnglishName = "Asyut", ArabicName = "أسيوط", EnglishCapital = "Asyut", ArabicCapital = "أسيوط", CountryId = 53 },
                     new Governorate {  EnglishName = "Luxor", ArabicName = "الأقصر", EnglishCapital = "Luxor", ArabicCapital = "الأقصر", CountryId = 53 },
                     new Governorate { EnglishName = "Red Sea", ArabicName = "البحر الأحمر", EnglishCapital = "Hurghada", ArabicCapital = "الغردقة", CountryId = 53 },
                     new Governorate {  EnglishName = "Beheira", ArabicName = "البحيرة", EnglishCapital = "Damanhur", ArabicCapital = "دمنهور", CountryId = 53 },
                     new Governorate { EnglishName = "Beni Suef", ArabicName = "بني سويف", EnglishCapital = "Beni Suef", ArabicCapital = "بني سويف", CountryId = 53 },
                     new Governorate {  EnglishName = "Port Said", ArabicName = "بورسعيد", EnglishCapital = "Port Said", ArabicCapital = "بورسعيد", CountryId = 53 },
                     new Governorate { EnglishName = "South Sinai", ArabicName = "جنوب سيناء", EnglishCapital = "El Tor", ArabicCapital = "الطور", CountryId = 53 },
                     new Governorate { EnglishName = "Giza", ArabicName = "الجيزة", EnglishCapital = "Giza", ArabicCapital = "الجيزة", CountryId = 53 },
                     new Governorate { EnglishName = "Dakahlia", ArabicName = "الدقهلية", EnglishCapital = "Mansoura", ArabicCapital = "المنصورة", CountryId = 53 },
                     new Governorate { EnglishName = "Damietta", ArabicName = "دمياط", EnglishCapital = "Damietta", ArabicCapital = "دمياط", CountryId = 53 },
                     new Governorate { EnglishName = "Sohag", ArabicName = "سوهاج", EnglishCapital = "Sohag", ArabicCapital = "سوهاج", CountryId = 53 },
                     new Governorate { EnglishName = "Suez", ArabicName = "السويس", EnglishCapital = "Suez", ArabicCapital = "السويس", CountryId = 53 },
                     new Governorate { EnglishName = "Sharqia", ArabicName = "الشرقية", EnglishCapital = "Zagazig", ArabicCapital = "الزقازيق", CountryId = 53 },
                     new Governorate { EnglishName = "North Sinai", ArabicName = "شمال سيناء", EnglishCapital = "Arish", ArabicCapital = "العريش", CountryId = 53 },
                     new Governorate { EnglishName = "Gharbia", ArabicName = "الغربية", EnglishCapital = "Tanta", ArabicCapital = "طنطا", CountryId = 53 },
                     new Governorate { EnglishName = "Faiyum", ArabicName = "الفيوم", EnglishCapital = "Faiyum", ArabicCapital = "الفيوم", CountryId = 53 },
                     new Governorate { EnglishName = "Cairo", ArabicName = "القاهرة", EnglishCapital = "Cairo", ArabicCapital = "القاهرة", CountryId = 53 },
                     new Governorate { EnglishName = "Qalyubia", ArabicName = "القليوبية", EnglishCapital = "Banha", ArabicCapital = "بنها", CountryId = 53 },
                     new Governorate { EnglishName = "Qina", ArabicName = "قنا", EnglishCapital = "Qina", ArabicCapital = "قنا", CountryId = 53 },
                     new Governorate { EnglishName = "Kafr El Sheikh", ArabicName = "كفر الشيخ", EnglishCapital = "Kafr El Sheikh", ArabicCapital = "كفر الشيخ", CountryId = 53 },
                     new Governorate { EnglishName = "Matruh", ArabicName = "مطروح", EnglishCapital = "Marsa Matruh", ArabicCapital = "مرسى مطروح", CountryId = 53 },
                     new Governorate { EnglishName = "Monufia", ArabicName = "المنوفية", EnglishCapital = "Shibin El Kom", ArabicCapital = "شبين الكوم", CountryId = 53 },
                     new Governorate { EnglishName = "Minya", ArabicName = "المنيا", EnglishCapital = "Minya", ArabicCapital = "المنيا", CountryId = 53 },
                     new Governorate { EnglishName = "New Valley", ArabicName = "الوادي الجديد", EnglishCapital = "Kharga", ArabicCapital = "الخارجة", CountryId = 53 }
                };
                await this._governorateRepository.InsertManyAsync(governorates);
            }
        }
    }


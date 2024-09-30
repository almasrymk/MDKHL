//using SmartParking.Entities.Lookups.VehiclesLookups;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Volo.Abp.Data;
//using Volo.Abp.DependencyInjection;
//using Volo.Abp.Domain.Repositories;

//namespace SmartParking.Data.Lookups;

//public class PlateColorDataSeeder : IDataSeedContributor, ITransientDependency
//{
//	private readonly IRepository<PlateColor, int> _plateColorRepository;

//	public PlateColorDataSeeder(IRepository<PlateColor, int> plateColorRepository)
//	{
//		_plateColorRepository = plateColorRepository;
//	}

//	public async Task SeedAsync(DataSeedContext context)
//	{
//		// Check if PlateColor data already exists
//		if (!await _plateColorRepository.AnyAsync())
//		{
//			// Define the list of plate colors to seed
//			var plateColors = new List<PlateColor>
//			{
//				new PlateColor {  EnglishName = "Red", ArabicName = "أحمر" },
//				new PlateColor {  EnglishName = "Blue", ArabicName = "أزرق" },
//				new PlateColor {  EnglishName = "Green", ArabicName = "أخضر" },
//				new PlateColor {  EnglishName = "Yellow", ArabicName = "أصفر" },
//				new PlateColor {  EnglishName = "White", ArabicName = "أبيض" }
//			};

//			// Insert the plate colors into the repository
//			await _plateColorRepository.InsertManyAsync(plateColors,autoSave:true);
//		}
//	}
//}

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

//public class VehicleBrandDataSeeder : IDataSeedContributor, ITransientDependency
//{
//	private readonly IRepository<VehicleBrand, int> _vehicleBrandRepository;

//	public VehicleBrandDataSeeder(IRepository<VehicleBrand, int> vehicleBrandRepository)
//	{
//		_vehicleBrandRepository = vehicleBrandRepository;
//	}

//	public async Task SeedAsync(DataSeedContext context)
//	{
//		// Check if VehicleBrand data already exists
//		if (!await _vehicleBrandRepository.AnyAsync())
//		{
//			// Define the list of vehicle brands to seed
//			var vehicleBrands = new List<VehicleBrand>
//			{
//				new VehicleBrand { EnglishName = "Honda", ArabicName = "هوندا" },
//				new VehicleBrand { EnglishName = "Toyota", ArabicName = "تويوتا" },
//				new VehicleBrand { EnglishName = "Tesla", ArabicName = "تسلا" },
//				new VehicleBrand { EnglishName = "Ford", ArabicName = "فورد" },
//				new VehicleBrand { EnglishName = "BMW", ArabicName = "بي إم دبليو" }
//			};

//			// Insert the vehicle brands into the repository
//			await _vehicleBrandRepository.InsertManyAsync(vehicleBrands);
//		}
//	}
//}

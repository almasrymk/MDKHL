using SmartParking.Entities.Lookups.VehiclesLookups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Data.Lookups;

public class VehicleBrandDataSeeder : IDataSeedContributor, ITransientDependency
{
	private readonly IRepository<VehicleBrand, int> _vehicleBrandRepository;

	public VehicleBrandDataSeeder(IRepository<VehicleBrand, int> vehicleBrandRepository)
	{
		_vehicleBrandRepository = vehicleBrandRepository;
	}

	public async Task SeedAsync(DataSeedContext context)
	{
		// Check if VehicleBrand data already exists
		if (!await _vehicleBrandRepository.AnyAsync())
		{
			// Define the list of vehicle brands to seed
			var vehicleBrands = new List<VehicleBrand>
			{
				new VehicleBrand { Id = 1, EnglishName = "Honda", ArabicName = "هوندا" },
				new VehicleBrand { Id = 2, EnglishName = "Toyota", ArabicName = "تويوتا" },
				new VehicleBrand { Id = 3, EnglishName = "Tesla", ArabicName = "تسلا" },
				new VehicleBrand { Id = 4, EnglishName = "Ford", ArabicName = "فورد" },
				new VehicleBrand { Id = 5, EnglishName = "BMW", ArabicName = "بي إم دبليو" }
			};

			// Insert the vehicle brands into the repository
			await _vehicleBrandRepository.InsertManyAsync(vehicleBrands);
		}
	}
}

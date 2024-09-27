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

public class VehicleModelDataSeeder : IDataSeedContributor, ITransientDependency
{
	private readonly IRepository<VehicleModel, int> _vehicleModelRepository;

	public VehicleModelDataSeeder(IRepository<VehicleModel, int> vehicleModelRepository)
	{
		_vehicleModelRepository = vehicleModelRepository;
	}

	public async Task SeedAsync(DataSeedContext context)
	{
		// Check if VehicleModel data already exists
		if (!await _vehicleModelRepository.AnyAsync())
		{
			// Define the list of vehicle models with their corresponding brands
			var vehicleModels = new List<VehicleModel>
			{
                    // Models for Honda (VehicleBrandId = 1)
                    new VehicleModel {  EnglishName = "Civic", ArabicName = "سيفيك", VehiclesBrandId = 1 },
				new VehicleModel {  EnglishName = "Accord", ArabicName = "أكورد", VehiclesBrandId = 1 },

                    // Models for Toyota (VehicleBrandId = 2)
                    new VehicleModel {EnglishName = "Corolla", ArabicName = "كورولا", VehiclesBrandId = 2 },
				new VehicleModel { EnglishName = "Camry", ArabicName = "كامري", VehiclesBrandId = 2 },

                    // Models for Tesla (VehicleBrandId = 3)
                    new VehicleModel {EnglishName = "Model S", ArabicName = "موديل اس", VehiclesBrandId = 3 },
				new VehicleModel { EnglishName = "Model 3", ArabicName = "موديل 3", VehiclesBrandId = 3 },

                    // Models for Ford (VehicleBrandId = 4)
                    new VehicleModel { EnglishName = "Mustang", ArabicName = "موستانغ", VehiclesBrandId = 4 },
				new VehicleModel {  EnglishName = "F-150", ArabicName = "اف-150", VehiclesBrandId = 4 },

                    // Models for BMW (VehicleBrandId = 5)
                    new VehicleModel {  EnglishName = "X5", ArabicName = "اكس 5", VehiclesBrandId = 5 },
				new VehicleModel {  EnglishName = "3 Series", ArabicName = "سلسلة 3", VehiclesBrandId = 5 }
			};

			// Insert the vehicle models into the repository
			await _vehicleModelRepository.InsertManyAsync(vehicleModels);
		}
	}
}

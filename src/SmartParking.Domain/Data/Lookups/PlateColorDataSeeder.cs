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

public class PlateColorDataSeeder : IDataSeedContributor, ITransientDependency
{
	private readonly IRepository<PlateColor, int> _plateColorRepository;

	public PlateColorDataSeeder(IRepository<PlateColor, int> plateColorRepository)
	{
		_plateColorRepository = plateColorRepository;
	}

	public async Task SeedAsync(DataSeedContext context)
	{
		// Check if PlateColor data already exists
		if (!await _plateColorRepository.AnyAsync())
		{
			// Define the list of plate colors to seed
			var plateColors = new List<PlateColor>
			{
				new PlateColor { Id = 1, EnglishName = "Red", ArabicName = "أحمر" },
				new PlateColor { Id = 2, EnglishName = "Blue", ArabicName = "أزرق" },
				new PlateColor { Id = 3, EnglishName = "Green", ArabicName = "أخضر" },
				new PlateColor { Id = 4, EnglishName = "Yellow", ArabicName = "أصفر" },
				new PlateColor { Id = 5, EnglishName = "White", ArabicName = "أبيض" }
			};

			// Insert the plate colors into the repository
			await _plateColorRepository.InsertManyAsync(plateColors);
		}
	}
}

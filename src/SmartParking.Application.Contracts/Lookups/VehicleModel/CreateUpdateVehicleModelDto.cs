using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Lookups.VehicleModel
{
	public class CreateUpdateVehicleModelDto
	{
		public string ArabicName { get; set; }
		public string EnglishName { get; set; }
		public int VehiclesBrandId { get; set; }

	}
}

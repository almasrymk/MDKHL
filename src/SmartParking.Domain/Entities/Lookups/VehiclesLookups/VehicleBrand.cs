using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Entities.Lookups.VehiclesLookups
{
	public class VehicleBrand:BaseEntity
	{
		public ICollection<VehicleModel> Models { get; set; }
	}
}

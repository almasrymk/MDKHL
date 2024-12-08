using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Entities.Lookups.VehiclesLookups;

public class VehicleModel:BaseEntity
{
	public int VehiclesBrandId { get; set; }
	public virtual VehicleBrand VehiclesBrand { get; set; }
}

using SmartParking.Entities.Cameras;
using SmartParking.Entities.Lookups.VehiclesLookups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Entities.Lookups
{
	public class PlateColor:BaseEntity
	{
		public virtual ICollection<LicenceType> Types { get; set; }
	}
}

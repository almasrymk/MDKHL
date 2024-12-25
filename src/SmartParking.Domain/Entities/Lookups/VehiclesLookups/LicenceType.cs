using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Entities.Lookups.VehiclesLookups
{
    public class LicenceType:BaseEntity
	{
        public int PlateColorId { get; set; }
        public virtual PlateColor PlateColor { get; set; }
        public string Image { get; set; }
    }
}

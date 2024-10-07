using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Vehicles
{
	public class CreateUpdateVehicleDto
	{
		public int VehicleModelId { get; set; }

        public Guid CustomerId { get; set; }
        public int VehicleBrandId { get; set; }
		public int PlateColorId { get; set; }
		public int LicenceTypeId { get; set; }
		public string ModelYear { get; set; }
		public bool BelongToCompany { get; set; }
		public string? CompanyName { get; set; }
        public string PlateNumber { get; set; }
        public Guid? TenantId { get; set; }
	}
}

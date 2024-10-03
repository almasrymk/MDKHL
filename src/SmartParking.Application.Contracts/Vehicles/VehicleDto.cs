using Abp.Domain.Entities.Auditing;
using SmartParking.Entities.Lookups.VehiclesLookups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Vehicles
{
	public class VehicleDto 
	{
		public int Id { get; set; }
		public int VehicleModelId { get; set; }
		public int VehicleBrandId { get; set; }
		public int PlateColorId { get; set; }
		public int LicenceTypeId { get; set; }
		public string ModelYear { get; set; }
		public bool BelongToCompany { get; set; }
		public string? CompanyName { get; set; }
        public string Platenumber { get; set; }
        public Guid? TenantId { get; set; }
	}
}

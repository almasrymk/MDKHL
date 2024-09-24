using SmartParking.Entities.Lookups;
using SmartParking.Entities.Lookups.VehiclesLookups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Entities.Vehicles
{
    public class Vehicle : FullAuditedEntity<int>, IMultiTenant
	{
        public int Id { get; set; }

        public virtual VehicleModel VehicleModel { get; set; }
		public virtual VehicleBrand VehicleBrand { get; set; }
		public virtual PlateColor PlateColor { get; set; }
		public virtual LicenceType LicenceType { get; set; }
        public string ModelYear { get; set; }
        public bool BelongToCompany { get; set; }
		public string? CompanyName { get; set; } 

        public Guid? TenantId { get; set; }

	}
}

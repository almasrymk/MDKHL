using SmartParking.Entities.Customers;
using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Entities.Vehicles
{
    public class Vehicle : FullAuditedEntity<int>, IMultiTenant,IBseEntity
	{
        public int Id { get; set; }
        public Guid CustomerId { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual VehicleModel VehicleModel { get; set; }
        public int VehicleModelId { get; set; }
        public virtual VehicleBrand VehicleBrand { get; set; }
		public int VehicleBrandId { get; set; }
        public string EnPlateNumber { get; set; }
        public string ArPlateNumber { get; set; }
        public virtual PlateColor PlateColor { get; set; }
		public int PlateColorId { get; set; }

		public virtual LicenceType LicenceType { get; set; }
		public int LicenceTypeId { get; set; }

		public string ModelYear { get; set; }
        public bool BelongToCompany { get; set; }
		public string? CompanyName { get; set; } 

        public Guid? TenantId { get; set; }
        public Status Status { get; set; }
    }
}

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
        public Guid CustomerId { get; set; }
        public int VehicleModelId { get; set; }
		public int VehicleBrandId { get; set; }
		public int PlateColorId { get; set; }
		public int LicenceTypeId { get; set; }
		public string ModelYear { get; set; }
		public string BrandArabicName { get; set; }
		public string ModelArabicName { get; set; }
		public string LicenceArabicName { get; set; }
        public string BrandEnglishName { get; set; }
        public string ModelEnglishName { get; set; }
		public string LicenceEnglishName { get; set; }
		public bool BelongToCompany { get; set; }
		public string? CompanyName { get; set; }
        public string ArPlateNumber { get; set; }
        public string EnPlateNumber { get; set; }
        public string ModelImage { get; set; }
        public string LicenceTypeImage { get; set; }
        public Guid? TenantId { get; set; }
	}
}

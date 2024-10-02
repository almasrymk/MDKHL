using SmartParking.Entities.Cameras;
using SmartParking.Entities.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Entities.GarageTransactions
{
	public class GarageTransaction : FullAuditedEntity<int>, IMultiTenant
	{

        public virtual Camera Camera { get; set; }
        public int CameraId { get; set; }
        public string PlateNnumber { get; set; }
        public DateTime? SnapTime { get; set; }
		public DateTime? AccurateTime { get; set; }
        public virtual Organization Organization { get; set; }
        public int OrganizationID { get; set; }
        public virtual Garage Garage { get; set; }
        public int GarageId { get; set; }
        public string VehiclePicture { get; set; }
        public string Type { get; set; }
        public Guid? TenantId { get; set; }

	}
}

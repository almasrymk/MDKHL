using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.GarageTransactions
{
	public class CreateUpdateGarageTransactionDto
	{
		public int CameraId { get; set; }
		public string PlateNnumber { get; set; }
		public DateTime? SnapTime { get; set; }
		public DateTime? AccurateTime { get; set; }
		public int OrganizationID { get; set; }
		public int GarageId { get; set; }
		public string VehiclePicture { get; set; }
		public string Type { get; set; }
		public Guid? TenantId { get; set; }
	}
}

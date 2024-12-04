using SmartParking.Enums;
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
		public string? PlateNumber { get; set; }
		public DateTime? SnapTime { get; set; }
		public DateTime? AccurateTime { get; set; }
		public DateTime? CheckoutMobile { get; set; }
		public int OrganizationID { get; set; }
		public int GarageId { get; set; }
		public string VehiclePicture { get; set; }
		public CameraType CameraType { get; set; }
		public Guid? TenantId { get; set; }
        public bool IsIn { get; set; }
		public CustomerCategoryStatus CustomerCategory { get; set; } = CustomerCategoryStatus.Customer;



    }
}
